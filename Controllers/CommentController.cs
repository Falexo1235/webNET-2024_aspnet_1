using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public CommentController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: api/comment/{id}/tree - Дерево комментариев (включая подкомментарии)
        [HttpGet("{id}/tree")]
public async Task<IActionResult> GetCommentTree(Guid id)
{
    // Получаем корневые комментарии, где ParentCommentId == null для поста с id
    var comments = await _context.Comments
        .Where(c => c.PostId == id && c.ParentId == null) // Это корневые комментарии
        .Include(c => c.Author)  // Загружаем автора комментария
        .ToListAsync();

    // Для каждого комментария подгружаем подкомментарии и их количество
    var result = new List<object>();

    foreach (var comment in comments)
    {
        // Загружаем подкомментарии
        var subComments = await _context.Comments
            .Where(c => c.ParentId == comment.Id)  // Проверяем, что ParentCommentId равен Id текущего комментария
            .ToListAsync();

        // Добавляем количество подкомментариев
        comment.SubComments = subComments.Count;

        // Убираем ParentCommentId, так как его не нужно отображать
        comment.ParentId = null;
        var commentAuthor = await _context.Users.FindAsync(comment.AuthorId);  // Найдем пользователя по AuthorId для комментариев
        comment.Author = commentAuthor?.FullName ?? "Unknown";  // Присваиваем имя

        // Преобразуем в нужную форму (для ответа)
        var commentDto = new
        {
            comment.Content,
            comment.ModifiedDate,
            comment.DeleteDate,
            comment.AuthorId,
            comment.Author, // Имя автора
            comment.SubComments,
            comment.Id,
            comment.CreateTime
        };

        result.Add(commentDto);

        // Рекурсивно добавляем подкомментарии в дерево
        if (subComments.Any())
        {
            // Вложенные комментарии
            var subCommentDtos = await GetSubComments(subComments);
            result.AddRange(subCommentDtos);
        }
    }

    return Ok(result);
}

// Метод для получения подкомментариев с вложенными подкомментариями
private async Task<IEnumerable<object>> GetSubComments(List<Comment> comments)
{
    var result = new List<object>();

    foreach (var comment in comments)
    {
        var subComments = await _context.Comments
            .Where(c => c.ParentId == comment.Id)
            .ToListAsync();

        // Считаем количество подкомментариев
        comment.SubComments = subComments.Count;
        
        // Убираем ParentCommentId, так как его не нужно отображать
        comment.ParentId = null;
        var commentAuthor = await _context.Users.FindAsync(comment.AuthorId);  // Найдем пользователя по AuthorId для комментариев
        comment.Author = commentAuthor?.FullName ?? "Unknown";  // Присваиваем имя

        var commentDto = new
        {
            comment.Content,
            comment.ModifiedDate,
            comment.DeleteDate,
            comment.AuthorId,
            comment.Author,
            comment.SubComments,
            comment.Id,
            comment.CreateTime
        };

        result.Add(commentDto);

        if (subComments.Any())
        {
            // Добавляем вложенные комментарии
            var subCommentDtos = await GetSubComments(subComments);
            result.AddRange(subCommentDtos);
        }
    }

    return result;
}


        

        // PUT: api/comment/{id} - Изменение комментария
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditComment(Guid id, [FromBody] CommentDto dto)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null) return NotFound("Comment not found.");

            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Проверка, что пользователь является автором комментария
            if (comment.AuthorId != userId)
                return Unauthorized("You can only edit your own comments.");

            comment.Content = dto.Content;
            comment.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Comment updated successfully." });
        }


        // DELETE: api/comment/{id} - Удаление комментария
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null) return NotFound("Comment not found.");

            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Проверка, что пользователь является автором комментария
            if (comment.AuthorId != userId)
                return Unauthorized("You can only delete your own comments.");

            comment.DeleteDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Comment deleted successfully." });
        }

    }
}
