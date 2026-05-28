using System.ComponentModel.DataAnnotations;
using DotnetAzureStarter.Core.Enums;

namespace DotnetAzureStarter.Core.Entities;

/// <summary>
/// Represents a todo item in the domain.
/// </summary>
public sealed class TodoItem : BaseEntity
{
    /// <summary>The title of the todo item.</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description providing more details about the todo item.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Indicates whether the todo item has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Optional due date for the todo item.</summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>Priority level of the todo item.</summary>
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
}
