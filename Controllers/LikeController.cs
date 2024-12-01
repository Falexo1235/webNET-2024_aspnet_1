using BlogApi.Data;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using BlogApi.DTOs;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public LikeController(BlogDbContext context)
        {
            _context = context;
        }

        // POST: /api/post/{postId}/like
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
