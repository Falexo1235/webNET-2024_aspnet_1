using BlogApi.Data;
using BlogApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using BlogApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация подключения к базе данных
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка аутентификации с JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost",
        ValidAudience = "https://localhost",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Регистрация NotificationBackgroundService для фоновых задач
builder.Services.AddScoped<NotificationBackgroundService>();  // Добавьте это для регистрации NotificationBackgroundService

// Настройка Quartz для фоновых задач
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();  // Настройка DI для Quartz
});

// Явная регистрация IScheduler
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.AddSingleton<IScheduler>(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    return schedulerFactory.GetScheduler().Result;
});

// Добавление контроллеров и сервисов
builder.Services.AddControllers();

// Регистрация сервисов для отправки уведомлений и подписок
builder.Services.AddSingleton<IEmailService, EmailService>();

// Настройка Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация HttpContextAccessor для получения текущего контекста запроса
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Настройка пайплайна в зависимости от окружения
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Включение middleware для проверки черного списка токенов
app.UseMiddleware<TokenBlacklistMiddleware>();

// Включение аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Маппинг контроллеров
app.MapControllers();

// Запуск приложения
app.Run();
