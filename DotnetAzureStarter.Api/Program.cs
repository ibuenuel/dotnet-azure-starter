using DotnetAzureStarter.Core.Interfaces;
using DotnetAzureStarter.Core.Interfaces.Services;
using DotnetAzureStarter.Infrastructure.Data;
using DotnetAzureStarter.Infrastructure.Options;
using DotnetAzureStarter.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI services
builder.Services.AddOpenApi();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// Configure CORS for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(corsBuilder =>
        {
            corsBuilder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
        });
    });
}

// Options Pattern — strongly typed database configuration
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

// EF Core — SQL Server via Options Pattern
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseSqlServer(db.ConnectionString, sql =>
        sql.CommandTimeout(db.CommandTimeoutSeconds));
});

// DI wiring — repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

// Run migrations and seed data on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    await DatabaseSeeder.SeedAsync(unitOfWork);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// TODO: Register an auth scheme before enabling (e.g. AddMicrosoftIdentityWebApi for Azure AD).
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();
