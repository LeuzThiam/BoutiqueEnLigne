using BoutiqueEnLigne.Catalog.Infrastructure;
using BoutiqueEnLigne.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.Error);

builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Catalog database connection string is not configured.");
builder.Services.AddCatalogInfrastructure(connectionString);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await EnsureDatabaseCreatedAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch when (attempt < 10)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
