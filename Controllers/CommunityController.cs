using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApi.Controllers
{
    [Route("api/community")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly BlogDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommunityController(BlogDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 1. Получить список всех сообществ
        [HttpGet]
        public async Task<IActionResult> GetCommunities()
        {
            var communities = await _context.Communities
                .Select(c => new CommunityFullDto
                {
                    Id = c.Id,
                    CreateTime = c.CreateTime,
                    Name = c.Name,
                    Description = c.Description,
                    IsClosed = c.IsClosed,
                    SubscribersCount = c.SubscribersCount,
                    Administrators = c.CommunityUsers
                        .Where(cu => cu.Role == "Administrator")
                        .Select(cu => new CommunityUserDto
                        {
                            UserId = cu.UserId,
                            CommunityId = cu.CommunityId,
                            Role = cu.Role
                        }).ToList()
                })
                .ToListAsync();

            return Ok(communities);
        }

        // 2. Получить список сообществ пользователя
        [HttpGet("my")]
        public async Task<IActionResult> GetUserCommunities()
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var userCommunities = await _context.CommunityUsers
                .Where(cu => cu.UserId == userId)
                .Include(cu => cu.Community)
                .Select(cu => new CommunityFullDto
                {
                    Id = cu.Community.Id,
                    CreateTime = cu.Community.CreateTime,
                    Name = cu.Community.Name,
                    Description = cu.Community.Description,
                    IsClosed = cu.Community.IsClosed,
                    SubscribersCount = cu.Community.SubscribersCount,
                    Administrators = cu.Community.CommunityUsers
                        .Where(cu => cu.Role == "Administrator")
                        .Select(cu => new CommunityUserDto
                        {
                            UserId = cu.UserId,
                            CommunityId = cu.CommunityId,
                            Role = cu.Role
                        }).ToList()
                })
                .ToListAsync();

            return Ok(userCommunities);
        }

        // 3. Получить информацию о сообществе
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommunityInfo(Guid id)
        {
            var community = await _context.Communities
                .Where(c => c.Id == id)
                .Select(c => new CommunityFullDto
                {
                    Id = c.Id,
                    CreateTime = c.CreateTime,
                    Name = c.Name,
                    Description = c.Description,
                    IsClosed = c.IsClosed,
                    SubscribersCount = c.SubscribersCount,
                    Administrators = c.CommunityUsers
                        .Where(cu => cu.Role == "Administrator")
                        .Select(cu => new CommunityUserDto
                        {
                            UserId = cu.UserId,
                            CommunityId = cu.CommunityId,
                            Role = cu.Role
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (community == null)
                return NotFound("Community not found.");

            return Ok(community);
        }

        // 4. Получить посты сообщества
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

        // 5. Создать пост в сообществе
        [HttpPost("{id}/post")]
        [Authorize]
        public async Task<IActionResult> CreatePost(Guid id, [FromBody] Post post)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var community = await _context.Communities
                .FirstOrDefaultAsync(c => c.Id == id);

            if (community == null)
                return NotFound("Community not found.");

            var newPost = new Post
            {
                Title = post.Title,
                Description = post.Description,
                AuthorId = userId,
                Author = post.Author,  // автор может быть добавлен при создании
                CommunityId = id,
                CommunityName = community.Name,
                CreateTime = DateTime.UtcNow
            };

            _context.Posts.Add(newPost);
            await _context.SaveChangesAsync();

            return Ok(newPost);
        }

        // 6. Получить роль пользователя в сообществе
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

        // 7. Подписать пользователя на сообщество
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

        // 8. Отписать пользователя от сообщества
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