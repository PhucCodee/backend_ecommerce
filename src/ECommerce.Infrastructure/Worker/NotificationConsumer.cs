using System.Globalization;
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
)
    : IConsumer<PaymentSucceededEvent>,
        IConsumer<PaymentFailedEvent>,
        IConsumer<OrderConfirmedEvent>,
        IConsumer<OrderShippedEvent>,
        IConsumer<UserRegisteredEvent>
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

        var subject = $"Payment Confirmation for Order #{message.OrderId}";
        var body = $"""
<table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f5; font-family:Arial, sans-serif;">
  <tr>
    <td align="center">

      <!-- Main container -->
      <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;">

        <!-- Banner -->
        <tr>
          <td>
            <img 
              src="https://pub-ce2153b8154b4936949a13f6f9ef8cb9.r2.dev/banner.JPG"
              width="600"
              height="60"
              alt="Banner"
              style="display:block; width:100%; height:60px; object-fit:cover; border:0;"
            />
          </td>
        </tr>

        <!-- Content -->
        <tr>
          <td style="padding:24px;">
            <h2 style="margin:0 0 16px 0; font-size:20px;">Payment Confirmation</h2>

            <p style="margin:0 0 12px 0;">
              Dear {user.UserProfile?.FirstName} {user.UserProfile?.LastName},
            </p>

            <p style="margin:0 0 16px 0;">
              We are pleased to inform you that your payment for order
              <strong>#{message.OrderId}</strong> has been successfully processed.
            </p>

            <p style="margin:0 0 8px 0;"><strong>Payment Details:</strong></p>

            <table width="100%" cellpadding="0" cellspacing="0" style="font-size:14px;">
              <tr>
                <td style="padding:4px 0;"><strong>Order Number: </strong></td>
                <td style="padding:4px 0;">{message.OrderId}</td>
              </tr>
              <tr>
                <td style="padding:4px 0;"><strong>Amount Paid: </strong></td>
                <td style="padding:4px 0;">
                  {message.Amount.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"))}
                </td>
              </tr>
            </table>

            <p style="margin:16px 0 0 0;">
              Thank you for shopping with us. If you have any questions, contact support.
            </p>

            <p style="margin:16px 0 0 0;">
              Best regards,<br/>
              <strong>Sanquo Shop</strong>
            </p>
          </td>
        </tr>

      </table>
    </td>
  </tr>
</table>
""";

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogWarning(
            "NotificationConsumer received payment.failed {EventId} for order {OrderId} user {UserId}. Reason={Reason}",
            message.EventId,
            message.OrderId,
            message.UserId,
            message.Reason
        );

        var user = await _dbContext
            .Users.Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.UserId == message.UserId);

        if (user is null)
        {
            _logger.LogError("User not found for user ID {UserId}", message.UserId);
            return;
        }

        var subject = $"Payment Failed for Order #{message.OrderId}";
        var body = $"""
            <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f5; font-family:Arial, sans-serif;">
              <tr>
                <td align="center">

                  <!-- Main container -->
                  <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;">

                    <!-- Banner -->
                    <tr>
                      <td>
                        <img 
                          src="https://pub-ce2153b8154b4936949a13f6f9ef8cb9.r2.dev/banner.JPG"
                          width="600"
                          height="60"
                          alt="Banner"
                          style="display:block; width:100%; height:60px; object-fit:cover; border:0;"
                        />
                      </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                      <td style="padding:24px;">
                        <h2 style="margin:0 0 16px 0; font-size:20px;">Payment Failed</h2>

                        <p style="margin:0 0 12px 0;">
                          Dear {user.UserProfile?.FirstName} {user.UserProfile?.LastName},
                        </p>

                        <p style="margin:0 0 16px 0;">
                          Unfortunately, your payment for order <strong>#{message.OrderId}</strong> could not be processed.
                        </p>

                        <p style="margin:0 0 8px 0;"><strong>Order Details:</strong></p>

                        <table width="100%" cellpadding="0" cellspacing="0" style="font-size:14px;">
                          <tr>
                            <td style="padding:4px 0;"><strong>Order Number: </strong></td>
                            <td style="padding:4px 0;">{message.OrderId}</td>
                          </tr>
                          <tr>
                            <td style="padding:4px 0;"><strong>Amount: </strong></td>
                            <td style="padding:4px 0;">
                              {message.Amount.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"))}
                            </td>
                          </tr>
                        </table>

                        <p style="margin:16px 0 0 0;">
                          Please try again or contact support if the issue continues.
                        </p>

                        <p style="margin:16px 0 0 0;">
                          Best regards,<br/>
                          <strong>Sanquo Shop</strong>
                        </p>
                      </td>
                    </tr>

                  </table>
                </td>
              </tr>
            </table>
            """;

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var message = context.Message;
        var user = await _dbContext
            .Users.Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.UserId == message.UserId);

        if (user is null)
            return;

        var subject = $"Order Confirmed #{message.OrderId}";
        var body = $"""
<table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f5; font-family:Arial, sans-serif;">
  <tr>
    <td align="center">

      <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;">

        <!-- Banner -->
        <tr>
          <td>
            <img 
              src="https://pub-ce2153b8154b4936949a13f6f9ef8cb9.r2.dev/banner.JPG"
              width="600"
              height="60"
              alt="Banner"
              style="display:block; width:100%; height:60px; object-fit:cover; border:0;"
            />
          </td>
        </tr>

        <!-- Content -->
        <tr>
          <td style="padding:24px;">
            <h2 style="margin:0 0 16px 0; font-size:20px;">Order Confirmed</h2>

            <p style="margin:0 0 12px 0;">
              Dear {user.UserProfile?.FirstName} {user.UserProfile?.LastName},
            </p>

            <p style="margin:0 0 16px 0;">
              Your order <strong>#{message.OrderId}</strong> has been confirmed.
            </p>

            <p style="margin:0;">
              We will notify you once it ships.
            </p>

            <p style="margin:16px 0 0 0;">
              Best regards,<br/>
              <strong>Sanquo Shop</strong>
            </p>
          </td>
        </tr>

      </table>
    </td>
  </tr>
</table>
""";

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var message = context.Message;
        var user = await _dbContext
            .Users.Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.UserId == message.UserId);

        if (user is null)
            return;

        var subject = $"Order Shipped #{message.OrderId}";
        var body = $"""
<table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f5; font-family:Arial, sans-serif;">
  <tr>
    <td align="center">

      <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;">

        <!-- Banner -->
        <tr>
          <td>
            <img 
              src="https://pub-ce2153b8154b4936949a13f6f9ef8cb9.r2.dev/banner.JPG"
              width="600"
              height="60"
              alt="Banner"
              style="display:block; width:100%; height:60px; object-fit:cover; border:0;"
            />
          </td>
        </tr>

        <!-- Content -->
        <tr>
          <td style="padding:24px;">
            <h2 style="margin:0 0 16px 0; font-size:20px;">Order Shipped</h2>

            <p style="margin:0 0 12px 0;">
              Dear {user.UserProfile?.FirstName} {user.UserProfile?.LastName},
            </p>

            <p style="margin:0 0 16px 0;">
              Your order <strong>#{message.OrderId}</strong> has shipped.
            </p>

            <p style="margin:0;">
              You can expect delivery soon.
            </p>

            <p style="margin:16px 0 0 0;">
              Best regards,<br/>
              <strong>Sanquo Shop</strong>
            </p>
          </td>
        </tr>

      </table>
    </td>
  </tr>
</table>
""";

        await _emailService.SendEmailAsync(user.Email, subject, body);
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "NotificationConsumer received user.registered {EventId} for user {UserId}",
            message.EventId,
            message.UserId
        );

        var subject = "Welcome to Sanquo Shop!";
        var body = $"""
<table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f5f5f5; font-family:Arial, sans-serif;">
  <tr>
    <td align="center">

      <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;">

        <!-- Banner -->
        <tr>
          <td>
            <img 
              src="https://pub-ce2153b8154b4936949a13f6f9ef8cb9.r2.dev/banner.JPG"
              width="600"
              height="60"
              alt="Banner"
              style="display:block; width:100%; height:60px; object-fit:cover; border:0;"
            />
          </td>
        </tr>

        <!-- Content -->
        <tr>
          <td style="padding:24px;">
            <h2 style="margin:0 0 16px 0; font-size:20px;">Welcome to Sanquo Shop!</h2>

            <p style="margin:0 0 12px 0;">
              Dear {message.FirstName} {message.LastName},
            </p>

            <p style="margin:0 0 16px 0;">
              Thank you for registering an account with us. We are thrilled to have you on board!
            </p>

            <p style="margin:0 0 16px 0;">
              Start exploring our products and enjoy seamless shopping right away.
            </p>

            <p style="margin:16px 0 0 0;">
              Best regards,<br/>
              <strong>Sanquo Shop</strong>
            </p>
          </td>
        </tr>

      </table>
    </td>
  </tr>
</table>
""";

        await _emailService.SendEmailAsync(message.Email, subject, body);
    }
}
