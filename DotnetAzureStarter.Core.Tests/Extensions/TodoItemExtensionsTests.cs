using DotnetAzureStarter.Core.DTOs;
using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Core.Enums;
using DotnetAzureStarter.Core.Extensions;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Extensions;

public sealed class TodoItemExtensionsTests
{
    private static TodoItem BuildEntity(
        string title = "Test Todo",
        bool isCompleted = false,
        DateTimeOffset? dueDate = null,
        TodoPriority priority = TodoPriority.Medium) =>
        new()
        {
            Title = title,
            Description = "A description",
            IsCompleted = isCompleted,
            DueDate = dueDate,
            Priority = priority
        };

    [Fact]
    public void ToDto_MapsAllFields_Correctly()
    {
        var entity = BuildEntity("Buy milk", dueDate: DateTimeOffset.UtcNow.AddDays(1), priority: TodoPriority.High);

        var dto = entity.ToDto();

        dto.Id.Should().Be(entity.Id);
        dto.Title.Should().Be(entity.Title);
        dto.Description.Should().Be(entity.Description);
        dto.IsCompleted.Should().Be(entity.IsCompleted);
        dto.DueDate.Should().Be(entity.DueDate);
        dto.Priority.Should().Be(entity.Priority);
        dto.CreatedAt.Should().Be(entity.CreatedAt);
        dto.UpdatedAt.Should().Be(entity.UpdatedAt);
    }

    [Fact]
    public void ToDto_IsOverdue_IsTrue_WhenDueDatePassed_AndNotCompleted()
    {
        var entity = BuildEntity(dueDate: DateTimeOffset.UtcNow.AddDays(-1), isCompleted: false);

        entity.ToDto().IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void ToDto_IsOverdue_IsFalse_WhenCompleted_EvenIfDueDatePassed()
    {
        var entity = BuildEntity(dueDate: DateTimeOffset.UtcNow.AddDays(-1), isCompleted: true);

        entity.ToDto().IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void ToEntity_MapsAllFields_Correctly()
    {
        var dto = new CreateTodoDto
        {
            Title = "Write tests",
            Description = "Cover all layers",
            DueDate = DateTimeOffset.UtcNow.AddDays(3),
            Priority = TodoPriority.High
        };

        var entity = dto.ToEntity();

        entity.Id.Should().NotBeEmpty();
        entity.Title.Should().Be(dto.Title);
        entity.Description.Should().Be(dto.Description);
        entity.DueDate.Should().Be(dto.DueDate);
        entity.Priority.Should().Be(dto.Priority);
        entity.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void UpdateFromDto_AppliesMutableFields_AndPreservesIdentity()
    {
        var entity = BuildEntity("Original title");
        var originalId = entity.Id;
        var originalCreatedAt = entity.CreatedAt;

        var updateDto = new UpdateTodoDto
        {
            Title = "Updated title",
            Description = "New description",
            IsCompleted = true,
            DueDate = DateTimeOffset.UtcNow.AddDays(5),
            Priority = TodoPriority.Low
        };

        entity.UpdateFromDto(updateDto);

        entity.Id.Should().Be(originalId);
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.Title.Should().Be(updateDto.Title);
        entity.Description.Should().Be(updateDto.Description);
        entity.IsCompleted.Should().BeTrue();
        entity.DueDate.Should().Be(updateDto.DueDate);
        entity.Priority.Should().Be(updateDto.Priority);
    }

    [Fact]
    public void ToDtoCollection_MapsAllItemsInOrder()
    {
        var items = new List<TodoItem>
        {
            BuildEntity("First"),
            BuildEntity("Second"),
            BuildEntity("Third")
        };

        var dtos = items.ToDtoCollection().ToList();

        dtos.Should().HaveCount(3);
        dtos[0].Title.Should().Be("First");
        dtos[1].Title.Should().Be("Second");
        dtos[2].Title.Should().Be("Third");
    }

    [Fact]
    public void ToDto_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        TodoItem? entity = null;

        var act = () => entity!.ToDto();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToEntity_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        CreateTodoDto? dto = null;

        var act = () => dto!.ToEntity();

        act.Should().Throw<ArgumentNullException>();
    }
}
