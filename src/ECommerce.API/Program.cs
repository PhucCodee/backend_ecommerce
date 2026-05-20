using System;
using System.IO;
using System.Text;
using System.Threading.RateLimiting;
using ECommerce.API.Logging;
using ECommerce.API.Middleware;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mappings;
using ECommerce.Application.Services;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Common;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Services.ZaloPay;
using ECommerce.Infrastructure.Worker;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Npgsql 6+ rejects mixed DateTime kinds in a single insert (UTC for
// CreatedAt/UpdatedAt vs Unspecified for date-picker fields like ValidFrom).
// All our timestamp columns are `TIMESTAMP WITHOUT TIME ZONE`, so the legacy
// behavior — treat all DateTime values as local/unspecified — is what we want.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── In-memory log buffer (must be registered before Serilog config) ───────────
var logBuffer = new InMemoryLogBuffer();
builder.Services.AddSingleton(logBuffer);

// ── Serilog: Console + in-memory sink ─────────────────────────────────────────
builder.Host.UseSerilog(
    (ctx, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Sink(new InMemoryLogSink(logBuffer));
    }
);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Accept and return enum values as strings (e.g. "percentage" not 0)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:SecretKey"]
                        ?? throw new InvalidOperationException("JWT SecretKey not configured")
                )
            ),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ECommerce",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ECommerce",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(
        "AuthPolicy",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        }
    );

    options.AddFixedWindowLimiter(
        "ApiPolicy",
        opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        }
    );

    options.AddFixedWindowLimiter(
        "UserActionPolicy",
        opt =>
        {
            opt.PermitLimit = 30;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        }
    );
});

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin))
    .AddPolicy(Policies.AdminOrSeller, policy => policy.RequireRole(Roles.Admin, Roles.Seller))
    .AddPolicy(Policies.SellerOnly, policy => policy.RequireRole(Roles.Seller))
    .AddPolicy(Policies.Authenticated, policy => policy.RequireAuthenticatedUser());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductSkuRepository, ProductSkuRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IOrderPaymentRepository, OrderPaymentRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<UserValidationHelper>();

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEventPublisher, EventPublisher>();
builder.Services.Configure<ZaloPayOptions>(builder.Configuration.GetSection("ZaloPay"));
builder.Services.AddHttpClient<IPaymentGatewayClient, ZaloPayPaymentGatewayClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.Configure<ECommerce.Infrastructure.Common.EmailSettings>(
    builder.Configuration.GetSection(ECommerce.Infrastructure.Common.EmailSettings.SectionName)
);
builder.Services.AddSingleton<IEmailService, SendGridEmailService>();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<PaymentConsumer>();
    x.AddConsumer<InventoryConsumer>();
    x.AddConsumer<NotificationConsumer>();

    x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(2);
    });

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            var rabbitMqHost = builder.Configuration["RabbitMQ:Host"];
            var rabbitMqVirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/";
            var rabbitMqUser = builder.Configuration["RabbitMQ:User"];
            var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"];
            var rabbitMqPort = ushort.TryParse(builder.Configuration["RabbitMQ:Port"], out var port)
                ? port
                : (ushort)5672;
            var rabbitMqUseSsl = builder.Configuration.GetValue<bool>("RabbitMQ:UseSsl");

            cfg.Host(
                rabbitMqHost,
                rabbitMqPort,
                rabbitMqVirtualHost,
                h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPassword);

                    if (rabbitMqUseSsl)
                    {
                        h.UseSsl(s =>
                        {
                            s.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                            s.ServerName = rabbitMqHost;
                        });
                    }
                }
            );

            cfg.ConfigureEndpoints(context);
        }
    );
});

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductQueryService, ProductQueryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductSkuQueryService, ProductSkuQueryService>();
builder.Services.AddScoped<IProductSkuService, ProductSkuService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

builder.Services.AddHttpClient();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();

// Serve uploaded files publicly — must be before auth middleware
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads",
    }
);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
