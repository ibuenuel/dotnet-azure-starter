using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.DTOs;
using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Core.Enums;
using DotnetAzureStarter.Core.Interfaces;
using DotnetAzureStarter.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace DotnetAzureStarter.Core.Tests.Services;

public sealed class TodoServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow = new();
    private readonly Mock<ITodoRepository> _mockRepo = new();
    private readonly TodoService _sut;

    public TodoServiceTests()
    {
        _mockUow.Setup(u => u.Todos).Returns(_mockRepo.Object);
        _sut = new TodoService(_mockUow.Object);
    }

    // ── GetAllAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsSuccess_WithMappedPagedResult()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "A", Priority = TodoPriority.High },
            new() { Title = "B", Priority = TodoPriority.Low }
        };
        var pagedEntities = new PagedResult<TodoItem>
        {
            Items = items,
            Page = 1,
            PageSize = 20,
            TotalCount = 2
        };
        _mockRepo
            .Setup(r => r.GetPagedAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedEntities);

        Result<PagedResult<TodoResponseDto>> result = await _sut.GetAllAsync(new PaginationRequest(1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_PassesPaginationToRepository()
    {
        var pagination = new PaginationRequest(2, 10);
        _mockRepo
            .Setup(r => r.GetPagedAsync(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<TodoItem> { Items = [], Page = 2, PageSize = 10, TotalCount = 0 });

        await _sut.GetAllAsync(pagination, CancellationToken.None);

        _mockRepo.Verify(r => r.GetPagedAsync(pagination, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WhenEntityExists()
    {
        var id = Guid.NewGuid();
        var entity = new TodoItem { Id = id, Title = "Existing todo" };
        _mockRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<TodoResponseDto> result = await _sut.GetByIdAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        result.Value.Title.Should().Be("Existing todo");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundError_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        Result<TodoResponseDto> result = await _sut.GetByIdAsync(id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    // ── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsSuccess_WithCreatedDto()
    {
        var dto = new CreateTodoDto
        {
            Title = "New todo",
            Description = "Details",
            Priority = TodoPriority.High
        };
        _mockRepo
            .Setup(r => r.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem entity, CancellationToken _) => entity);

        Result<TodoResponseDto> result = await _sut.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be(dto.Title);
        result.Value.Priority.Should().Be(dto.Priority);
    }

    [Fact]
    public async Task CreateAsync_CallsAddAndCommit()
    {
        var dto = new CreateTodoDto { Title = "Test" };
        _mockRepo
            .Setup(r => r.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem e, CancellationToken _) => e);

        await _sut.CreateAsync(dto, CancellationToken.None);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        var act = async () => await _sut.CreateAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WithUpdatedDto()
    {
        var id = Guid.NewGuid();
        var entity = new TodoItem { Id = id, Title = "Old title" };
        var dto = new UpdateTodoDto { Title = "New title", IsCompleted = true, Priority = TodoPriority.Low };
        _mockRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _mockRepo
            .Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result<TodoResponseDto> result = await _sut.UpdateAsync(id, dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New title");
        result.Value.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFoundError_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        Result<TodoResponseDto> result = await _sut.UpdateAsync(id, new UpdateTodoDto { Title = "x" }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_WhenEntityExists()
    {
        var id = Guid.NewGuid();
        var entity = new TodoItem { Id = id, Title = "To delete" };
        _mockRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        Result result = await _sut.DeleteAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockRepo.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFoundError_WhenEntityDoesNotExist()
    {
        var id = Guid.NewGuid();
        _mockRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        Result result = await _sut.DeleteAsync(id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
