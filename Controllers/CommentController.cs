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
    // Получаем комментарий-родитель
    var parentComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

    if (parentComment == null)
        return NotFound("Comment not found.");

    var commentTree = new List<object>();
    await BuildCommentTree(parentComment, commentTree);

    return Ok(commentTree);
}

// Рекурсивный метод построения дерева комментариев
private async Task BuildCommentTree(Comment comment, List<object> result)
{
    // Загружаем автора комментария
    var author = await _context.Users.FindAsync(comment.AuthorId);

    // Добавляем комментарий в результат
    result.Add(new
    {
        comment.Content,
        comment.ModifiedDate,
        comment.DeleteDate,
        comment.AuthorId,
        Author = author?.FullName ?? "Unknown",
        SubComments = await CountSubComments(comment.Id), // Рекурсивный подсчёт вложенных комментариев
        comment.Id,
        comment.CreateTime
    });

    // Получаем подкомментарии
    var subComments = await _context.Comments
        .Where(c => c.ParentId == comment.Id)
        .OrderByDescending(c => c.CreateTime)
        .ToListAsync();

    foreach (var subComment in subComments)
    {
        await BuildCommentTree(subComment, result);
    }
}
private async Task<int> CountSubComments(Guid commentId)
{
    var subComments = await _context.Comments
        .Where(c => c.ParentId == commentId)
        .ToListAsync();

    int count = subComments.Count;

    foreach (var subComment in subComments)
    {
        count += await CountSubComments(subComment.Id); // Рекурсивно подсчитываем вложенные комментарии
    }

    return count;
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
