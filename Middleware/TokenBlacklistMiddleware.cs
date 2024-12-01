using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Data;  // Для доступа к BlogDbContext

namespace BlogApi.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory; // Для создания области

        public TokenBlacklistMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Получаем токен из заголовка Authorization
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            // Если токен отсутствует, пропускаем запрос
            if (string.IsNullOrEmpty(token))
            {
                await _next(context);
                return;
            }

            // Создаем область для разрешения зависимостей (scoped)
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

                // Проверяем, есть ли токен в черном списке
                var revokedToken = await _context.RevokedTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token);

                if (revokedToken != null && revokedToken.ExpirationDate > DateTime.UtcNow)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            // Если токен не отозван, пропускаем запрос дальше
            await _next(context);
        }
    }
}
