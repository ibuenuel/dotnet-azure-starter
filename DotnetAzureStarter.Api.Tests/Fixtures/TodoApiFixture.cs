using DotnetAzureStarter.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace DotnetAzureStarter.Api.Tests.Fixtures;

/// <summary>
/// Shared test fixture that starts a real SQL Server container and a WebApplicationFactory.
/// All tests in a class sharing this fixture run against the same database instance.
/// Each test is responsible for creating its own data — do not rely on shared state.
/// </summary>
public sealed class TodoApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host =>
            {
                host.UseEnvironment("Testing");
                host.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:ConnectionString"] = _container.GetConnectionString()
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        if (_factory is not null)
            await _factory.DisposeAsync();

        await _container.DisposeAsync();
    }
}
