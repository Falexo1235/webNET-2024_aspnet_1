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

        [HttpGet("{id}/tree")]
public async Task<IActionResult> GetCommentTree(Guid id)
{
    var parentComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
    if (parentComment == null)
        return NotFound("Comment not found.");

    var commentTree = new List<object>();
    await BuildCommentTree(parentComment, commentTree);

    return Ok(commentTree);
}

private async Task BuildCommentTree(Comment comment, List<object> result)
{
    var author = await _context.Users.FindAsync(comment.AuthorId);
    result.Add(new
    {
        comment.Content,
        comment.ModifiedDate,
        comment.DeleteDate,
        comment.AuthorId,
        Author = author?.FullName ?? "Unknown",
        SubComments = await CountSubComments(comment.Id),
        comment.Id,
        comment.CreateTime
    });
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
        count += await CountSubComments(subComment.Id); 
    }

    return count;
}

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditComment(Guid id, [FromBody] CommentDto dto)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound("Comment not found.");
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            if (comment.AuthorId != userId)
                return Unauthorized("You can only edit your own comments.");

            if (comment.DeleteDate != null)
                return BadRequest("Deleted comments cannot be edited.");

            comment.Content = dto.Content;
            comment.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Comment updated successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return NotFound("Comment not found.");

            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            if (comment.AuthorId != userId)
                return Unauthorized("You can only delete your own comments.");

            if (comment.DeleteDate != null)
                return BadRequest("Comment has already been deleted.");

            comment.DeleteDate = DateTime.UtcNow;
            comment.Content = string.Empty;
            var subCommentsCount = await CountSubComments(comment.Id);
            if (subCommentsCount == 0)
            {
                _context.Comments.Remove(comment);
            }
            else
            {
                _context.Comments.Update(comment);
            }
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Comment deleted successfully." });
        }
    }
}
