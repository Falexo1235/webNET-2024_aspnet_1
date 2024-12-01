using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public PostController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: api/post
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var postsQuery = _context.Posts
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .AsQueryable();

            var totalPosts = await postsQuery.CountAsync();
            var posts = await postsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.CreateTime,
                    p.Title,
                    p.Description,
                    p.ReadingTime,
                    p.Image,
                    AuthorId = p.AuthorId,
                    p.AddressId,
                    Likes = p.Likes.Count,
                    CommentsCount = p.Comments.Count,  // Возвращаем количество комментариев
                    Tags = p.Tags.Select(t => new { t.Id, t.Name, t.CreateTime }).ToList()
                })
                .ToListAsync();

            var pagination = new
            {
                Size = pageSize,
                Count = totalPosts,
                Current = page
            };

            return Ok(new { Posts = posts, Pagination = pagination });
        }

        // GET: api/post/{id} - Получение поста с массивом комментариев
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)  // Загружаем комментарии
                .FirstOrDefaultAsync(p => p.Id == id);
            var postAuthor = await _context.Users.FindAsync(post.AuthorId);  // Найдем пользователя по AuthorId для комментариев
                post.Author = postAuthor?.FullName ?? "Unknown";  // Присваиваем имя

            if (post == null)
                return NotFound("Post not found.");

            // Для каждого комментария считаем количество подкомментариев
            foreach (var comment in post.Comments)
            {
                var commentAuthor = await _context.Users.FindAsync(comment.AuthorId);  // Найдем пользователя по AuthorId для комментариев
                 comment.Author = commentAuthor?.FullName ?? "Unknown";  // Присваиваем имя

                // Подсчитываем количество подкомментариев для каждого комментария
                comment.SubComments = await _context.Comments
                    .Where(c => c.ParentId == comment.Id)  // Проверяем, что ParentId равен Id текущего комментария
                    .CountAsync();  // Подсчитываем количество подкомментариев

                // Убираем ParentId, так как его не нужно отображать
                comment.ParentId = null;
            }

            return Ok(post);
        }

[HttpPost("{id}/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CommentDto dto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var comment = new Comment
            {
                Content = dto.Content,
                CreateTime = DateTime.UtcNow,
                AuthorId = userId,
                PostId = id,
                ParentId = dto.ParentId,  // Если это ответ на комментарий
                ModifiedDate = null,
                DeleteDate = null
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Comment added successfully." });
        }



        // POST: api/post
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (dto.Tags == null || !dto.Tags.Any())
                return BadRequest("At least one tag must be specified.");

            // Проверка на корректность URL изображения
            if (!string.IsNullOrEmpty(dto.Image) && !Uri.IsWellFormedUriString(dto.Image, UriKind.Absolute))
            {
                return BadRequest("Invalid image URL.");
            }

            // Извлекаем GUID автора из токена
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
                return Unauthorized("Invalid token.");

            var post = new Post
            {
                Id = Guid.NewGuid(),  // Генерация нового GUID
                Title = dto.Title,
                Description = dto.Description,
                ReadingTime = dto.ReadingTime,
                Image = dto.Image,
                AddressId = dto.AddressId,
                Tags = await _context.Tags.Where(t => dto.Tags.Contains(t.Id)).ToListAsync(),
                AuthorId = authorId,
                CreateTime = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Post created successfully." });
        }
    }
}
