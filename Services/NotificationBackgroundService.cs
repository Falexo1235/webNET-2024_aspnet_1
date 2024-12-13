using BlogApi.Data;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services
{    
        public class NotificationBackgroundService : BackgroundService
        {
            private readonly IServiceScopeFactory _serviceScopeFactory;
            
            public NotificationBackgroundService(IServiceScopeFactory serviceScopeFactory)
            {
                _serviceScopeFactory = serviceScopeFactory;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            public async Task SendPostNotificationsAsync(Post post)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                    var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var subscribers = await _context.CommunityUsers
                        .Where(cu => cu.CommunityId == post.CommunityId)
                        .ToListAsync();

                    var emails = new List<string>();
                    foreach (var subscriber in subscribers)
                    {
                        var user = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == subscriber.UserId);

                        if (user != null)
                        {
                            emails.Add(user.Email);
                        }
                    }
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "email_template.html");
                var htmlContent = await File.ReadAllTextAsync(templatePath);
                htmlContent = htmlContent
                    .Replace("{{postTitle}}", post.Title)
                    .Replace("{{postUrl}}", "http://example.com/post/"+post.Id)
                    .Replace("{{communityName}}", post.CommunityName);
                    foreach (var email in emails)
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(email, "New Post Notification", htmlContent);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
                        }
                    }
                }
            }
        }

}