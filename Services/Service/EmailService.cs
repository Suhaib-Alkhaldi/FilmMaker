using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FilmMaker.Services.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendOtpAsync(string toEmail, string otp, OtpPurpose purpose)
        {
            var subject = purpose == OtpPurpose.EmailVerification
                ? "Verify your email"
                : "Reset your password";

            var body = purpose == OtpPurpose.EmailVerification
                ? $"Your verification code is: <b>{otp}</b>. It expires in 10 minutes."
                : $"Your password reset code is: <b>{otp}</b>. It expires in 10 minutes.";

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
