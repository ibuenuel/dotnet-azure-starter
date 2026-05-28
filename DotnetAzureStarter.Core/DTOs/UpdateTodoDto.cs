using System.ComponentModel.DataAnnotations;
using DotnetAzureStarter.Core.Enums;

namespace DotnetAzureStarter.Core.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing todo item.
/// </summary>
public class UpdateTodoDto
{
    /// <summary>The title of the todo item.</summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description providing more details about the todo item.</summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>Indicates whether the todo item has been completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Optional due date for the todo item.</summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>Priority level of the todo item.</summary>
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
}
