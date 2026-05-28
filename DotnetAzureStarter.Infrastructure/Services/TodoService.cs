using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.DTOs;
using DotnetAzureStarter.Core.Extensions;
using DotnetAzureStarter.Core.Interfaces;
using DotnetAzureStarter.Core.Interfaces.Services;

namespace DotnetAzureStarter.Infrastructure.Services;

/// <summary>
/// Application service implementing business logic for TodoItem operations.
/// All data access goes through IUnitOfWork — never directly to a DbContext.
/// </summary>
public sealed class TodoService : ITodoService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Initializes the service with the unit of work.</summary>
    public TodoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResult<TodoResponseDto>>> GetAllAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Todos.GetPagedAsync(pagination, cancellationToken);

        var result = new PagedResult<TodoResponseDto>
        {
            Items = paged.Items.Select(t => t.ToDto()).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };

        return Result<PagedResult<TodoResponseDto>>.Success(result);
    }

    /// <inheritdoc/>
    public async Task<Result<TodoResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Todos.GetByIdAsync(id, cancellationToken);

        return entity is null
            ? Result<TodoResponseDto>.Failure(Error.NotFound($"Todo item with ID '{id}' was not found."))
            : Result<TodoResponseDto>.Success(entity.ToDto());
    }

    /// <inheritdoc/>
    public async Task<Result<TodoResponseDto>> CreateAsync(
        CreateTodoDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = dto.ToEntity();
        await _unitOfWork.Todos.AddAsync(entity, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<TodoResponseDto>.Success(entity.ToDto());
    }

    /// <inheritdoc/>
    public async Task<Result<TodoResponseDto>> UpdateAsync(
        Guid id,
        UpdateTodoDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var entity = await _unitOfWork.Todos.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<TodoResponseDto>.Failure(Error.NotFound($"Todo item with ID '{id}' was not found."));

        entity.UpdateFromDto(dto);
        await _unitOfWork.Todos.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<TodoResponseDto>.Success(entity.ToDto());
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Todos.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure(Error.NotFound($"Todo item with ID '{id}' was not found."));

        await _unitOfWork.Todos.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
