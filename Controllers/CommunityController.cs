using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BlogApi.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlogApi.Controllers
{
    [Route("api/community")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly BlogDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory;  // Поле для IServiceScopeFactory

        // Внедрение зависимостей через конструктор
        public CommunityController(BlogDbContext context, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));  // Инициализация
        }

        [HttpGet]
        public async Task<IActionResult> GetCommunities()
        {
            var communities = await _context.Communities
                .Select(c => new CommunityFullDto
                {
                    Name = c.Name,
                    Description = c.Description,
                    IsClosed = c.IsClosed,
                    SubscribersCount = c.SubscribersCount,
                    Id = c.Id,
                    CreateTime = c.CreateTime
                })
                .ToListAsync();
            return Ok(communities);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetUserCommunities()
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var userCommunities = await _context.CommunityUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => new CommunityUserDto
                {
                    UserId = cu.UserId,
                    CommunityId = cu.CommunityId,
                    Role = cu.Role
                })
                .ToListAsync();
            return Ok(userCommunities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommunityById(Guid id)
        {
            var community = await _context.Communities
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.IsClosed,
                    c.CreateTime,
                    SubscribersCount = _context.CommunityUsers.Count(cu => cu.CommunityId == c.Id),
                    Administrators = _context.Users
                        .Where(u => _context.CommunityUsers.Any(cu => cu.CommunityId == c.Id && cu.UserId == u.Id && cu.Role == "Administrator"))
                        .Select(u => new
                        {
                            u.Id,
                            u.CreationTime,
                            u.FullName,
                            u.BirthDate,
                            u.Gender,
                            u.Email,
                            u.Phone
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (community == null)
                return NotFound("Community not found.");

            return Ok(community);
        }

        [HttpGet("{id}/post")]
        public async Task<IActionResult> GetCommunityPosts(Guid id)
        {
            var posts = await _context.Posts
                .Where(p => p.CommunityId == id)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.CreateTime,
                    p.CommunityName,
                    p.Author,
                    p.Comments,
                    p.Tags,
                    LikesCount = p.Likes.Count
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost("{id}/post")]
        [Authorize]
        public async Task<IActionResult> CreatePost(Guid id, [FromBody] CreatePostDto dto)
        {
            if (dto.Tags == null || !dto.Tags.Any())
                return BadRequest("At least one tag must be specified.");

            if (!string.IsNullOrEmpty(dto.Image) && !Uri.IsWellFormedUriString(dto.Image, UriKind.Absolute))
            {
                return BadRequest("Invalid image URL.");
            }

            var community = await _context.Communities
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound("Community not found.");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
                return Unauthorized("Invalid token.");

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                ReadingTime = dto.ReadingTime,
                Image = dto.Image,
                AddressId = dto.AddressId,
                Tags = await _context.Tags.Where(t => dto.Tags.Contains(t.Id)).ToListAsync(),
                AuthorId = authorId,
                CommunityId = id,
                CommunityName = community.Name,
                CreateTime = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Фоновая задача отправки уведомлений
            
            Task.Run(async () =>
                {
                    // Фоновая задача для отправки уведомлений
                    await SendPostNotificationsAsync(post);
                });

            return Ok(new { Message = "Post created successfully." });
        }
        private async Task SendPostNotificationsAsync(Post post){
            using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var backgroundService = scope.ServiceProvider.GetRequiredService<NotificationBackgroundService>();
                            await backgroundService.SendPostNotificationsAsync(post);  // Метод для отправки уведомлений
                        }
        }

        [HttpGet("{id}/role")]
        public async Task<IActionResult> GetUserRoleInCommunity(Guid id)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var role = await _context.CommunityUsers
                .Where(cu => cu.UserId == userId && cu.CommunityId == id)
                .Select(cu => cu.Role)
                .FirstOrDefaultAsync();
            return Ok(role ?? (object)null);
        }

        [HttpPost("{id}/subscribe")]
        [Authorize]
        public async Task<IActionResult> Subscribe(Guid id)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var existingSubscription = await _context.CommunityUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CommunityId == id);

            if (existingSubscription != null)
                return BadRequest("User is already subscribed.");

            var communityUser = new CommunityUser
            {
                UserId = userId,
                CommunityId = id,
                Role = "Subscriber"
            };

            _context.CommunityUsers.Add(communityUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}/unsubscribe")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(Guid id)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var subscription = await _context.CommunityUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CommunityId == id);

            if (subscription == null)
                return BadRequest("User is not subscribed to this community.");

            _context.CommunityUsers.Remove(subscription);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}