using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Core.Enums;
using DotnetAzureStarter.Core.Interfaces;
using DotnetAzureStarter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetAzureStarter.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for TodoItem entities.
/// Extends GenericRepository with domain-specific query operations.
/// </summary>
public sealed class TodoRepository : GenericRepository<TodoItem>, ITodoRepository
{
    /// <summary>Initializes the todo repository with the shared database context.</summary>
    public TodoRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoItem>> GetCompletedAsync(CancellationToken cancellationToken = default)
        => await _context.TodoItems
            .AsNoTracking()
            .Where(t => t.IsCompleted)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoItem>> GetPendingAsync(CancellationToken cancellationToken = default)
        => await _context.TodoItems
            .AsNoTracking()
            .Where(t => !t.IsCompleted)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(
        TodoPriority priority,
        CancellationToken cancellationToken = default)
        => await _context.TodoItems
            .AsNoTracking()
            .Where(t => t.Priority == priority)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoItem>> GetDueOrOverdueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.TodoItems
            .AsNoTracking()
            .Where(t => t.DueDate <= now && !t.IsCompleted)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TodoItem>> SearchAsync(
        string searchText,
        CancellationToken cancellationToken = default)
        => await _context.TodoItems
            .AsNoTracking()
            .Where(t => t.Title.Contains(searchText) ||
                        (t.Description != null && t.Description.Contains(searchText)))
            .ToListAsync(cancellationToken);
}
