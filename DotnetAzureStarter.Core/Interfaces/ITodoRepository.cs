using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Core.Enums;

namespace DotnetAzureStarter.Core.Interfaces;

/// <summary>
/// Specialised repository for TodoItem entities.
/// Extends the generic repository with todo-specific query operations.
/// </summary>
public interface ITodoRepository : IRepository<TodoItem>
{
    /// <summary>Returns all completed todo items.</summary>
    Task<IReadOnlyList<TodoItem>> GetCompletedAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all pending (not completed) todo items.</summary>
    Task<IReadOnlyList<TodoItem>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns todo items matching the specified priority level.</summary>
    Task<IReadOnlyList<TodoItem>> GetByPriorityAsync(TodoPriority priority, CancellationToken cancellationToken = default);

    /// <summary>Returns todo items that are due today or already overdue.</summary>
    Task<IReadOnlyList<TodoItem>> GetDueOrOverdueAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns todo items whose title or description contains the search text.</summary>
    Task<IReadOnlyList<TodoItem>> SearchAsync(string searchText, CancellationToken cancellationToken = default);
}
