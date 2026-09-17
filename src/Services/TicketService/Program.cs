using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TicketService.Data;
using TicketService.Repositories;
using TicketService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers and string enums
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

// PostgreSQL
builder.Services.AddDbContext<TicketDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("TicketDatabase");

    options.UseNpgsql(connectionString);
});

// Repository, Unit of Work and Service
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITicketsService, TicketsService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITicketHistoryRepository, TicketHistoryRepository>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
// JWT authentication
var userManagementUrl =
    builder.Configuration["Services:UserManagementService"]
    ?? throw new InvalidOperationException("UserManagementService URL is not configured.");

builder.Services.AddHttpClient<UserManagementClient>(
    client =>
    {
        client.BaseAddress =new Uri(userManagementUrl);

        client.Timeout =TimeSpan.FromSeconds(5);
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
                ValidIssuer =builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience =builder.Configuration["Jwt:Audience"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();