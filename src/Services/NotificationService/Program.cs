using HelpDesk.NotificationService.Consumers;
using HelpDesk.NotificationService.Data;
using IntegrationContracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "NotificationDatabase")));

var rabbit = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TicketCreatedConsumer>();
    x.AddConsumer<TicketAssignedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            rabbit["Host"] ?? "localhost",
            rabbit["VirtualHost"] ?? "/",
            h =>
            {
                h.Username(rabbit["Username"] ?? "helpdesk");
                h.Password(
                    rabbit["Password"]
                    ?? throw new InvalidOperationException(
                        "RabbitMQ password is missing."));
            });


        cfg.ReceiveEndpoint("notification.ticket-created", e =>
        {
            e.ConfigureConsumer<TicketCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification.ticket-assigned", e =>
        {
            e.ConfigureConsumer<TicketAssignedConsumer>(context);
        });
    });
});
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
