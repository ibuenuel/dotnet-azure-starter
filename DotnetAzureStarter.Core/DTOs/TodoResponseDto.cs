using DotnetAzureStarter.Core.Enums;

namespace DotnetAzureStarter.Core.DTOs;

/// <summary>
/// Data Transfer Object for todo item responses.
/// </summary>
public class TodoResponseDto
{
    /// <summary>Unique identifier for the todo item.</summary>
    public Guid Id { get; set; }

    /// <summary>The title of the todo item.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description providing more details about the todo item.</summary>
    public string? Description { get; set; }

    /// <summary>Indicates whether the todo item has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Optional due date for the todo item.</summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>Priority level of the todo item.</summary>
    public TodoPriority Priority { get; set; }

    /// <summary>UTC timestamp when the todo item was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>UTC timestamp when the todo item was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Indicates if the todo item is past its due date and not yet completed.</summary>
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTimeOffset.UtcNow && !IsCompleted;
}
