using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            // Получаем всех пользователей, у которых есть хотя бы один пост
            var authors = await _context.Users
                .Where(u => _context.Posts.Any(p => p.AuthorId == u.Id))  // Убедимся, что есть хотя бы один пост
                .Select(u => new
                {
                    u.FullName,
                    u.BirthDate,
                    u.Gender,
                    u.CreationTime,
                    PostsCount = _context.Posts.Count(p => p.AuthorId == u.Id),  // Подсчитываем количество постов
                    LikesCount = _context.Posts
                        .Where(p => p.AuthorId == u.Id)
                        .Sum(p => p.Likes.Count)  // Подсчитываем количество лайков для всех постов автора
                })
                .ToListAsync();

            return Ok(authors);
        }
    }
}