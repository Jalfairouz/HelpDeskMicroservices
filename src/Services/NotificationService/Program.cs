using HelpDesk.NotificationService.Consumers;
using IntegrationContracts.Events;

using HelpDesk.NotificationService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "NotificationDatabase")));

var rabbit = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TicketCreatedConsumer>();

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
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapControllers();

await app.RunAsync();
