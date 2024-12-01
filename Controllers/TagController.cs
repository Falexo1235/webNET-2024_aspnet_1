using BlogApi.Data;
using BlogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // GET: /api/tag
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            // Получаем список всех тегов из базы данных без использования PostId
            var tags = await _context.Tags.ToListAsync();

            // Возвращаем результат
            return Ok(tags);
        }
    }
}
