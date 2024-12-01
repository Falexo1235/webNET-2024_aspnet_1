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
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly BlogDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(BlogDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: /api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Неверный логин или пароль.");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),  // Генерация нового GUID
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        // POST: /api/account/logout (выход из системы)
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();  // Получаем токен из заголовка
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is missing.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized("Invalid token.");

            // Получаем дату истечения токена
            var expirationDate = jwtToken.ValidTo;

            // Добавляем токен в черный список
            var revokedToken = new RevokedToken
            {
                Token = token,
                ExpirationDate = expirationDate
            };

            _context.RevokedTokens.Add(revokedToken);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Successfully logged out." });
        }

        // GET: /api/account/profile (получение профиля пользователя)
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token.");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            var userProfile = new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Phone,
                user.DateOfBirth,
                user.Gender
            };

            return Ok(userProfile);
        }

        // PUT: /api/account/profile (изменение данных пользователя)
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token.");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            // Убедитесь, что дата рождения передается в UTC
            if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value.Kind == DateTimeKind.Unspecified)
            {
                // Если дата имеет 'Unspecified' тип, установите UTC
                dto.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc);
            }

            // Обновление данных пользователя
            user.FullName = dto.FullName ?? user.FullName;
            user.Phone = dto.Phone ?? user.Phone;
            user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
            user.Gender = dto.Gender ?? user.Gender;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Profile updated successfully." });
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())  // Используем GUID как строку
                },
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
