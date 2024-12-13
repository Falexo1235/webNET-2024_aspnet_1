using MimeKit;
using MailKit.Net.Smtp;

namespace BlogApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {

            if (string.IsNullOrEmpty(to))
    {
        throw new ArgumentException("Email address cannot be null or empty.", nameof(to));
    }
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Blog System", _configuration["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress(to, to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], 587, false);
                await client.AuthenticateAsync(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
