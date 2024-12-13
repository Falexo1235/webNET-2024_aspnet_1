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
            // Извлекаем все теги, используя правильную модель
            var tags = await _context.Tags
                .Include(t => t.PostTags)  // Включаем связанные записи в PostTags
                .ThenInclude(pt => pt.Post)  // Включаем связанные записи Post (если нужно)
                .ToListAsync();

            return Ok(tags);
        }
    }
}
