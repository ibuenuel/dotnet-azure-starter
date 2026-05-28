using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Core.Enums;
using DotnetAzureStarter.Core.Interfaces;

namespace DotnetAzureStarter.Infrastructure.Data;

/// <summary>
/// Seeds the database with sample data for local development.
/// Only runs when the TodoItems table is empty — safe to call on every startup.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>Seeds sample todo items if the table is empty.</summary>
    /// <param name="unitOfWork">The unit of work providing repository access.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    public static async Task SeedAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        var existing = await unitOfWork.Todos.GetPagedAsync(new PaginationRequest(1, 1), cancellationToken);
        if (existing.TotalCount > 0)
            return;

        var now = DateTimeOffset.UtcNow;

        var todos = new List<TodoItem>
        {
            new()
            {
                Title = "Set up Azure Key Vault",
                Description = "Configure Azure Key Vault and migrate all secrets from environment variables.",
                Priority = TodoPriority.High,
                DueDate = now.AddDays(3),
                IsCompleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Title = "Write integration tests for Todos API",
                Description = "Cover the full CRUD lifecycle using WebApplicationFactory and a real SQL Server container.",
                Priority = TodoPriority.High,
                DueDate = now.AddDays(7),
                IsCompleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Title = "Configure GitHub Actions CI pipeline",
                Description = "Set up build, test, and vulnerability scan steps for pull request validation.",
                Priority = TodoPriority.Medium,
                DueDate = now.AddDays(14),
                IsCompleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Title = "Write Bicep modules for App Service",
                Description = "Provision Azure App Service Plan (F1) and Web App via infrastructure as code.",
                Priority = TodoPriority.Medium,
                DueDate = now.AddDays(21),
                IsCompleted = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Title = "Add .editorconfig",
                Description = "Enforce consistent code style across editors.",
                Priority = TodoPriority.Low,
                DueDate = null,
                IsCompleted = true,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-1)
            }
        };

        foreach (var todo in todos)
            await unitOfWork.Todos.AddAsync(todo, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
