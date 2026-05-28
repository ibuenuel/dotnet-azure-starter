using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.DTOs;
using DotnetAzureStarter.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAzureStarter.Api.Controllers;

/// <summary>
/// REST API endpoints for managing todo items.
/// </summary>
[ApiController]
[Route("api/todos")]
public sealed class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    /// <summary>Initializes the controller with the todo service.</summary>
    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    /// <summary>Returns a paginated list of all todo items.</summary>
    /// <param name="page">1-based page number. Defaults to 1.</param>
    /// <param name="pageSize">Items per page. Defaults to 20.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TodoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoService.GetAllAsync(
            new PaginationRequest(page, pageSize),
            cancellationToken);

        return result.Match(
            onSuccess: paged => Ok(ApiResponse<PagedResult<TodoResponseDto>>.Ok(paged)),
            onFailure: error => error.Type switch
            {
                ErrorType.NotFound => NotFound(ApiResponse<PagedResult<TodoResponseDto>>.Fail(error)),
                ErrorType.Validation => UnprocessableEntity(ApiResponse<PagedResult<TodoResponseDto>>.Fail(error)),
                _ => StatusCode(500, ApiResponse<PagedResult<TodoResponseDto>>.Fail(error))
            });
    }

    /// <summary>Returns a single todo item by ID.</summary>
    /// <param name="id">The unique identifier of the todo item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _todoService.GetByIdAsync(id, cancellationToken);

        return result.Match(
            onSuccess: dto => Ok(ApiResponse<TodoResponseDto>.Ok(dto)),
            onFailure: error => error.Type switch
            {
                ErrorType.NotFound => NotFound(ApiResponse<TodoResponseDto>.Fail(error)),
                _ => StatusCode(500, ApiResponse<TodoResponseDto>.Fail(error))
            });
    }

    /// <summary>Creates a new todo item.</summary>
    /// <param name="dto">The todo item data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTodoDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoService.CreateAsync(dto, cancellationToken);

        return result.Match(
            onSuccess: created => CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<TodoResponseDto>.Ok(created)),
            onFailure: error => error.Type switch
            {
                ErrorType.Validation => UnprocessableEntity(ApiResponse<TodoResponseDto>.Fail(error)),
                _ => StatusCode(500, ApiResponse<TodoResponseDto>.Fail(error))
            });
    }

    /// <summary>Updates an existing todo item.</summary>
    /// <param name="id">The unique identifier of the todo item.</param>
    /// <param name="dto">The updated todo item data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<TodoResponseDto>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTodoDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoService.UpdateAsync(id, dto, cancellationToken);

        return result.Match(
            onSuccess: updated => Ok(ApiResponse<TodoResponseDto>.Ok(updated)),
            onFailure: error => error.Type switch
            {
                ErrorType.NotFound => NotFound(ApiResponse<TodoResponseDto>.Fail(error)),
                ErrorType.Validation => UnprocessableEntity(ApiResponse<TodoResponseDto>.Fail(error)),
                _ => StatusCode(500, ApiResponse<TodoResponseDto>.Fail(error))
            });
    }

    /// <summary>Deletes a todo item by ID.</summary>
    /// <param name="id">The unique identifier of the todo item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _todoService.DeleteAsync(id, cancellationToken);

        return result.Match(
            onSuccess: () => (IActionResult)NoContent(),
            onFailure: error => error.Type switch
            {
                ErrorType.NotFound => NotFound(ApiResponse<object>.Fail(error)),
                _ => StatusCode(500, ApiResponse<object>.Fail(error))
            });
    }
}
