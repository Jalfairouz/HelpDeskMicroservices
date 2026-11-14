using System.Text;
using LoggingService.Consumers;
using LoggingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IApiLogStore, ElasticLogStore>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHostedService<ElasticBootstrapper>();

var rabbit = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ApiRequestBatchConsumer, ApiRequestBatchConsumerDefinition>();
    x.AddConsumer<ApiResponseBatchConsumer, ApiResponseBatchConsumerDefinition>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbit["Host"] ?? "localhost", rabbit["VirtualHost"] ?? "/", h =>
        {
            h.Username(rabbit["Username"] ?? "helpdesk");
            h.Password(rabbit["Password"]
                ?? throw new InvalidOperationException("RabbitMQ password is missing."));
        });

        cfg.PrefetchCount = 300; 
        cfg.ConfigureEndpoints(context);
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();