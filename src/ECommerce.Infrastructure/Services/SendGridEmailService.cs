using System;
using System.Threading.Tasks;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ECommerce.Infrastructure.Services;

public class SendGridEmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<SendGridEmailService> logger
) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly ILogger<SendGridEmailService> _logger = logger;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.ApiKey))
        {
            _logger.LogError("SendGrid API key is not configured.");
            return;
        }

        try
        {
            var client = new SendGridClient(_emailSettings.ApiKey);
            var from = new EmailAddress(_emailSettings.FromAddress, _emailSettings.FromName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, "", body);

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent to {To} via SendGrid.", to);
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send email via SendGrid. Status: {StatusCode}, Body: {ResponseBody}",
                    response.StatusCode,
                    responseBody
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An exception occurred while sending email to {To} via SendGrid.",
                to
            );
        }
    }
}
