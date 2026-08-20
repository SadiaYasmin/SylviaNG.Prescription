using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Infrastructure.Services
{
    /// <summary>
    /// SMTP mail sender (MailKit) for everything this backend emails directly — OTP codes
    /// for forgot-password/change-email/change-password confirmation. Doctor/Staff account
    /// invitations use Keycloak's own realm SMTP config instead (execute-actions-email), not
    /// this service, so the two "who sends what" paths stay independent of each other.
    /// </summary>
    public class MailKitEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailKitEmailService> _logger;

        public MailKitEmailService(IConfiguration configuration, ILogger<MailKitEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var host = _configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
            var port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var username = _configuration["Email:Username"] ?? throw new InvalidOperationException("Email:Username is not configured.");
            var password = _configuration["Email:Password"] ?? throw new InvalidOperationException("Email:Password is not configured.");
            var fromAddress = _configuration["Email:FromAddress"] ?? username;
            var fromName = _configuration["Email:FromName"] ?? "PrescriptionMS";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
                await client.AuthenticateAsync(username, password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true, cancellationToken);
            }

            _logger.LogInformation("Sent email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
        }
    }
}
