using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.DTOs;

namespace DotnetAzureStarter.Core.Interfaces.Services;

/// <summary>
/// Business logic layer for TodoItem operations.
/// Sits between API controllers and the repository layer.
/// Controllers call this interface — never repositories directly.
/// </summary>
public interface ITodoService
{
    /// <summary>Returns a paginated list of all todo items.</summary>
    Task<Result<PagedResult<TodoResponseDto>>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single todo item by ID. Returns a NotFound error if it does not exist.</summary>
    Task<Result<TodoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates and persists a new todo item.</summary>
    Task<Result<TodoResponseDto>> CreateAsync(CreateTodoDto dto, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing todo item. Returns a NotFound error if it does not exist.</summary>
    Task<Result<TodoResponseDto>> UpdateAsync(Guid id, UpdateTodoDto dto, CancellationToken cancellationToken = default);

    /// <summary>Deletes a todo item by ID. Returns a NotFound error if it does not exist.</summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
