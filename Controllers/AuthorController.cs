using BlogApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [Route("api/author")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public AuthorController(BlogDbContext context)
        {
            _context = context;
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetAuthorsWithPosts()
        {
            var authors = await _context.Users
                .Where(u => _context.Posts.Any(p => p.AuthorId == u.Id))
                .Select(u => new
                {
                    u.FullName,
                    u.BirthDate,
                    u.Gender,
                    u.CreationTime,
                    PostsCount = _context.Posts.Count(p => p.AuthorId == u.Id),
                    LikesCount = _context.Posts
                        .Where(p => p.AuthorId == u.Id)
                        .Sum(p => p.Likes.Count)
                })
                .ToListAsync();
            return Ok(authors);
        }
    }
}