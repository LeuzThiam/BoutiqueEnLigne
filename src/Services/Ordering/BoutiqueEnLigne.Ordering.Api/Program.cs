using System.Text;
using BoutiqueEnLigne.Ordering.Infrastructure;
using BoutiqueEnLigne.Ordering.Infrastructure.Persistence;
using BoutiqueEnLigne.EventBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Ordering database connection string is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret is not configured.");
if (jwtSecret.Length < 32) throw new InvalidOperationException("JWT secret must contain at least 32 characters.");
builder.Services.AddOrderingInfrastructure(connectionString);
builder.Services.AddRabbitMqEventBus(new RabbitMqOptions
{
    Host = builder.Configuration["EventBus:Host"] ?? "localhost",
    Port = builder.Configuration.GetValue("EventBus:Port", 5672),
    UserName = builder.Configuration["EventBus:UserName"] ?? "guest",
    Password = builder.Configuration["EventBus:Password"] ?? "guest",
    VirtualHost = builder.Configuration["EventBus:VirtualHost"] ?? "/",
    ExchangeName = builder.Configuration["EventBus:ExchangeName"] ?? "boutique.events"
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    });
builder.Services.AddAuthorization();
builder.Services.AddServiceObservability(builder.Configuration, "boutique-ordering");

var app = builder.Build();
app.UseServiceObservability();

await EnsureDatabaseCreatedAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureDatabaseCreatedAsync(WebApplication app)
{
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch when (attempt < 10)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
