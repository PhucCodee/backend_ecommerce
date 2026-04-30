using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Worker;

public sealed class NotificationConsumer(
    ILogger<NotificationConsumer> logger,
    IEmailService emailService,
    ApplicationDbContext dbContext
) : IConsumer<PaymentSucceededEvent>, IConsumer<PaymentFailedEvent>
{
    private readonly ILogger<NotificationConsumer> _logger = logger;
    private readonly IEmailService _emailService = emailService;
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "NotificationConsumer received payment.succeeded {EventId} for order {OrderId} user {UserId}",
            message.EventId,
            message.OrderId,
            message.UserId
        );

        var user = await _dbContext
            .Users.Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.UserId == message.UserId);

        if (user is null)
        {
            _logger.LogError("User not found for user ID {UserId}", message.UserId);
            return;
        }

        var subject = $"Payment Confirmation for Order #{message.OrderNumber}";
        var body = $"""
            <h1>Payment Confirmation</h1>
            <p>Dear {user.UserProfile?.FirstName ?? user.Username},</p>
            <p>We are pleased to inform you that your payment for order <strong>#{message.OrderNumber}</strong> has been successfully processed.</p>
            <p><strong>Payment Details:</strong></p>
            <ul>
                <li><strong>Order Number:</strong> {message.OrderNumber}</li>
                <li><strong>Amount Paid:</strong> {message.Amount:C}</li>
                <li><strong>Transaction ID:</strong> {message.TransactionId}</li>
            </ul>
            <p>Thank you for shopping with us. If you have any questions or need assistance, please contact our support team.</p>
            <p>Best regards,<br/>Sanquo Shop</p>
            """;

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

    public Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogWarning(
            "NotificationConsumer received payment.failed {EventId} for order {OrderId} user {UserId}. Reason={Reason}",
            message.EventId,
            message.OrderId,
            message.UserId,
            message.Reason
        );

        // Optionally, send a notification email about the failed payment
        return Task.CompletedTask;
    }
}
