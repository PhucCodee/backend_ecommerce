using System.Threading.Tasks;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Common;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace ECommerce.Infrastructure.Services;

public class SmtpEmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<SmtpEmailService> logger
) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly ILogger<SmtpEmailService> _logger = logger;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromAddress));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // Depending on the policy, you might want to re-throw or handle this differently
        }
    }
}
