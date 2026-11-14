using ApiGateway.Logging;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddApiLogging();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Correlation-Id"); 
    });
});

var rabbit = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbit["Host"] ?? "localhost", rabbit["VirtualHost"] ?? "/", h =>
        {
            h.Username(rabbit["Username"] ?? "helpdesk");
            h.Password(rabbit["Password"]
                ?? throw new InvalidOperationException("RabbitMQ password is missing."));
        });
    });
});

builder.Services.AddSingleton<ApiLogQueue>();
builder.Services.AddHostedService<ApiLogPublisher>();

var app = builder.Build();

app.MapGet("/", () => "API Gateway is running");
app.UseCors("AllowAll");
app.MapReverseProxy();

app.Run();