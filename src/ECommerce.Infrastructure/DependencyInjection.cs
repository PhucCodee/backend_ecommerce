using ECommerce.Domain.Interfaces;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Common;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Services.ZaloPay;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        _ = services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        _ = services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        _ = services.AddSingleton<IEmailService>(sp =>
        {
            var emailSettings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            return emailSettings.Provider switch
            {
                "SendGrid" => new SendGridEmailService(
                    sp.GetRequiredService<IOptions<EmailSettings>>(),
                    loggerFactory.CreateLogger<SendGridEmailService>()
                ),
                "Smtp" => new SmtpEmailService(
                    sp.GetRequiredService<IOptions<EmailSettings>>(),
                    loggerFactory.CreateLogger<SmtpEmailService>()
                ),
                _ => throw new System.Exception(
                    $"Email provider '{emailSettings.Provider}' is not supported."
                ),
            };
        });

        _ = services.AddScoped<IUnitOfWork>(sp =>
            (IUnitOfWork)sp.GetRequiredService<ApplicationDbContext>()
        );

        _ = services.AddHttpClient<IPaymentGatewayClient, ZaloPayPaymentGatewayClient>();

        _ = services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();
            busConfig.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.Host(
                        "rabbitmq",
                        "/",
                        h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        }
                    );
                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        return services;
    }
}
