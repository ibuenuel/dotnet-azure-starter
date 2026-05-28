using DotnetAzureStarter.Core.DTOs;
using DotnetAzureStarter.Core.Entities;

namespace DotnetAzureStarter.Core.Extensions;

/// <summary>
/// Extension methods for mapping between TodoItem entities and DTOs.
/// </summary>
public static class TodoItemExtensions
{
    /// <summary>Converts a TodoItem entity to a TodoResponseDto.</summary>
    public static TodoResponseDto ToDto(this TodoItem todoItem)
    {
        ArgumentNullException.ThrowIfNull(todoItem);

        return new TodoResponseDto
        {
            Id = todoItem.Id,
            Title = todoItem.Title,
            Description = todoItem.Description,
            IsCompleted = todoItem.IsCompleted,
            DueDate = todoItem.DueDate,
            Priority = todoItem.Priority,
            CreatedAt = todoItem.CreatedAt,
            UpdatedAt = todoItem.UpdatedAt
        };
    }

    /// <summary>Converts a CreateTodoDto to a new TodoItem entity.</summary>
    public static TodoItem ToEntity(this CreateTodoDto createDto)
    {
        ArgumentNullException.ThrowIfNull(createDto);

        return new TodoItem
        {
            Title = createDto.Title,
            Description = createDto.Description,
            DueDate = createDto.DueDate,
            Priority = createDto.Priority,
            IsCompleted = false
        };
    }

    /// <summary>Updates an existing TodoItem entity with values from UpdateTodoDto.</summary>
    public static TodoItem UpdateFromDto(this TodoItem todoItem, UpdateTodoDto updateDto)
    {
        ArgumentNullException.ThrowIfNull(todoItem);
        ArgumentNullException.ThrowIfNull(updateDto);

        todoItem.Title = updateDto.Title;
        todoItem.Description = updateDto.Description;
        todoItem.IsCompleted = updateDto.IsCompleted;
        todoItem.DueDate = updateDto.DueDate;
        todoItem.Priority = updateDto.Priority;

        return todoItem;
    }

    /// <summary>Converts a collection of TodoItem entities to a TodoResponseDto collection.</summary>
    public static IEnumerable<TodoResponseDto> ToDtoCollection(this IEnumerable<TodoItem> todoItems)
    {
        ArgumentNullException.ThrowIfNull(todoItems);
        return todoItems.Select(item => item.ToDto());
    }
}
