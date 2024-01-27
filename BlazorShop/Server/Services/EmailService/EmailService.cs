using BlazorShop.Shared.Entities;
using MailKit.Net.Smtp;
using MimeKit;

namespace BlazorShop.Server.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public void SendEmail(Email request)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("BS Scooter", _config.GetSection("EmailConfig:EmailUserName").Value));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = request.Body };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailConfig:EmailHost").Value, 587, MailKit.Security.SecureSocketOptions.StartTls); //dodac smtp server z gmail
            smtp.Authenticate(_config.GetSection("EmailConfig:EmailUserName").Value, _config.GetSection("EmailConfig:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
