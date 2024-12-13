using BlogApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly BlogDbContext _context;
        public TagController(BlogDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _context.Tags
                .Include(t => t.PostTags)
                .ThenInclude(pt => pt.Post)
                .ToListAsync();

            return Ok(tags);
        }
    }
}
