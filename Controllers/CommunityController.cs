using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BlogApi.Services;
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
        public async Task<IActionResult> GetCommunityPosts(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sorting = null,
            [FromQuery] List<Guid>? tags = null) // Добавлен параметр для тегов
        {
            // Базовый запрос для постов сообщества
            var postsQuery = _context.Posts
                .Where(p => p.CommunityId == id)
                .Include(p => p.PostTags)  // Включаем связь с PostTags
                .Include(p => p.Comments)
                .AsQueryable();

            // Фильтрация по тегам: выбираем только те посты, у которых есть все теги из переданного списка
            if (tags != null && tags.Any())
            {
                postsQuery = postsQuery.Where(p =>
                    tags.All(tagId => p.PostTags.Any(pt => pt.TagId == tagId)));
            }

            // Сортировка
            postsQuery = sorting switch
            {
                "CreateAsc" => postsQuery.OrderBy(p => p.CreateTime),
                "CreateDesc" => postsQuery.OrderByDescending(p => p.CreateTime),
                "LikesAsc" => postsQuery.OrderBy(p => _context.Likes.Count(l => l.PostId == p.Id)),
                "LikesDesc" => postsQuery.OrderByDescending(p => _context.Likes.Count(l => l.PostId == p.Id)),
                _ => postsQuery.OrderByDescending(p => p.CreateTime) // По умолчанию сортировка по убыванию даты создания
            };

            // Подсчет общего количества постов для пагинации
            var totalPosts = await postsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            // Получение постов для текущей страницы
            var posts = await postsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.CreateTime,
                    p.CommunityName,
                    Author = _context.Users.FirstOrDefault(u => u.Id == p.AuthorId).FullName,
                    CommentsCount = p.Comments.Count,
                    Tags = p.PostTags.Select(pt => new { pt.Tag.Id, pt.Tag.Name, pt.Tag.CreateTime }).ToList(),
                    LikesCount = _context.Likes.Count(l => l.PostId == p.Id)
                })
                .ToListAsync();

            var pagination = new
            {
                Size = pageSize,
                Count = totalPages,
                Current = page
            };

            return Ok(new { Posts = posts, Pagination = pagination });
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
                AddressId = dto.AddressId == Guid.Empty ? null : dto.AddressId,
                AuthorId = authorId,
                CommunityId = id,
                CommunityName = community.Name,
                CreateTime = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Получаем теги из базы данных по переданным Id
            var tags = await _context.Tags.Where(t => dto.Tags.Contains(t.Id)).ToListAsync();

            // Создаем связи между постом и тегами через таблицу PostTags
            var postTags = tags.Select(tag => new PostTag
            {
                PostId = post.Id,
                TagId = tag.Id
            }).ToList();

            _context.PostTags.AddRange(postTags);
            await _context.SaveChangesAsync();

            Task.Run(async () =>
            {
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
            var community = await _context.Communities.FirstOrDefaultAsync(c => c.Id == id);
            if (community == null)
                return NotFound("Community not found.");
            var communityUser = new CommunityUser
            {
                UserId = userId,
                CommunityId = id,
                Role = "Subscriber"
            };

            _context.CommunityUsers.Add(communityUser);
            await _context.SaveChangesAsync();

            return Ok($"You are now subscribed to \"{community.Name}\"");
        }

        [HttpDelete("{id}/unsubscribe")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(Guid id)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var subscription = await _context.CommunityUsers
                .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CommunityId == id);
            var community = await _context.Communities.FirstOrDefaultAsync(c => c.Id == id);
            if (community == null)
                return NotFound("Community not found.");
            if (subscription == null)
                return BadRequest("User is not subscribed to this community.");
            _context.CommunityUsers.Remove(subscription);
            await _context.SaveChangesAsync();
            return Ok($"You are now unsubscribed from \"{community.Name}\"");
        }
    }
}