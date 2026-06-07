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
        private readonly ILogger _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }


        public async Task SendOtpAsync(string toEmail, string otp, OtpPurpose purpose)
        {
            var host = _config["Email:Host"] ?? throw new InvalidOperationException("Email Host is not configured.");
            var portStr = _config["Email:Port"] ?? throw new InvalidOperationException("Email Port is not configured.");
            var fromEmail = _config["Email:From"] ?? throw new InvalidOperationException("Email From address is not configured.");
            var username = _config["Email:Username"] ?? throw new InvalidOperationException("Email Username is not configured.");
            var password = _config["Email:Password"] ?? throw new InvalidOperationException("Email Password is not configured.");

            if (!int.TryParse(portStr, out int port))
            {
                throw new InvalidOperationException("Email Port must be a valid integer.");
            }

            var isVerification = purpose == OtpPurpose.EmailVerification;
            var subject = isVerification ? "Verify your email" : "Reset your password";
            var actionText = isVerification ? "verification" : "password reset";
            var body = $"Your {actionText} code is: <b>{otp}</b>. It expires in 10 minutes.";

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                throw; 
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}
