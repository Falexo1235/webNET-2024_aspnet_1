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
            var userId = User.Identity.IsAuthenticated 
                ? Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value) 
                : Guid.Empty; // Получаем ID пользователя из claims (если он авторизован)

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
                    p.Title,
                    p.Description,
                    p.ReadingTime,
                    p.Image,
                    AuthorId = p.AuthorId,
                    p.CommunityId,
                    p.CommunityName,
                    p.AddressId,
                    Likes = _context.Likes.Count(l => l.PostId == p.Id),
                    HasLike = userId != Guid.Empty 
                        ? _context.Likes.Any(l => l.PostId == p.Id && l.UserId == userId) 
                        : false,  // Проверка лайка только для авторизованных пользователей
                    CommentsCount = p.Comments.Count,  // Возвращаем количество комментариев
                    Tags = p.Tags.Select(t => new { t.Id, t.Name, t.CreateTime }).ToList(),
                    p.Id,
                    p.CreateTime,
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

    if (post == null)
        return NotFound("Post not found.");

    var postAuthor = await _context.Users.FindAsync(post.AuthorId);
    post.Author = postAuthor?.FullName ?? "Unknown";

    // Для каждого комментария считаем количество подкомментариев
    int totalCommentCount = 0;
    foreach (var comment in post.Comments.Where(c => c.ParentId == null))
    {
        var commentAuthor = await _context.Users.FindAsync(comment.AuthorId);
        comment.Author = commentAuthor?.FullName ?? "Unknown";

        comment.SubComments = await CountSubCommentsRecursively(comment.Id);

        totalCommentCount += comment.SubComments + 1;
    }
     bool hasLike = false;
    if (User.Identity.IsAuthenticated)
    {
        var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        hasLike = await _context.Likes.AnyAsync(l => l.PostId == post.Id && l.UserId == userId);
    }
    return Ok(new
    {
        Comments = post.Comments.Where(c => c.ParentId == null) // Только основные комментарии
            .Select(c => new
            {
                c.Content,
                c.ModifiedDate,
                c.DeleteDate,
                c.AuthorId,
                c.Author,
                c.SubComments,
                c.Id,
                c.CreateTime

            }),
        post.Title,
        post.Description,
        post.ReadingTime,
        post.Image,
        post.AuthorId,
        post.Author,
        post.CommunityId,
        post.CommunityName,
        Likes = await _context.Likes.CountAsync(l => l.PostId == post.Id), // Количество лайков
        HasLike = hasLike,
        CommentsCount = totalCommentCount,
        post.Tags,
        post.Id,
        post.CreateTime,
    });
}
// Рекурсивный метод подсчёта подкомментариев
private async Task<int> CountSubCommentsRecursively(Guid parentId)
{
    var directSubComments = await _context.Comments
        .Where(c => c.ParentId == parentId)
        .ToListAsync();

    var totalCount = directSubComments.Count;

    foreach (var subComment in directSubComments)
    {
        totalCount += await CountSubCommentsRecursively(subComment.Id);
    }

    return totalCount;
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

        
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> AddLike(Guid postId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Проверка: пользователь уже лайкнул пост?
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existingLike != null)
                return BadRequest("You have already liked this post.");

            var like = new Like
            {
                PostId = postId,
                UserId = userId
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Post liked successfully." });
        }


        // DELETE: /api/post/{postId}/like
        [HttpDelete("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> RemoveLike(Guid postId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Найти существующий лайк
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null)
                return NotFound("Like not found.");

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Like removed successfully." });
        }
    }
}
