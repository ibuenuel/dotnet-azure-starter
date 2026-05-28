using System.Net;
using System.Net.Http.Json;
using DotnetAzureStarter.Api.Tests.Fixtures;
using DotnetAzureStarter.Core.Common;
using DotnetAzureStarter.Core.DTOs;
using DotnetAzureStarter.Core.Enums;
using FluentAssertions;

namespace DotnetAzureStarter.Api.Tests.Controllers;

public sealed class TodosControllerIntegrationTests : IClassFixture<TodoApiFixture>
{
    private readonly HttpClient _client;

    public TodosControllerIntegrationTests(TodoApiFixture fixture)
    {
        _client = fixture.Client;
    }

    // ── Health check ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheck_Returns200Healthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }

    // ── GET /api/todos ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Returns200_WithValidEnvelope()
    {
        var response = await _client.GetAsync("/api/todos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<TodoResponseDto>>>();
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsPaginationMetadata()
    {
        var response = await _client.GetAsync("/api/todos?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<TodoResponseDto>>>();
        envelope!.Data!.PageSize.Should().Be(5);
        envelope.Data.Page.Should().Be(1);
    }

    // ── GET /api/todos/{id} ────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_Returns200_WithCorrectDto_WhenExists()
    {
        var created = await CreateTodoAsync(new CreateTodoDto
        {
            Title = "GetById test",
            Priority = TodoPriority.High
        });

        var response = await _client.GetAsync($"/api/todos/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        envelope!.Success.Should().BeTrue();
        envelope.Data!.Id.Should().Be(created.Id);
        envelope.Data.Title.Should().Be("GetById test");
    }

    [Fact]
    public async Task GetById_Returns404_WithNotFoundError_WhenDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        envelope!.Success.Should().BeFalse();
        envelope.ErrorCode.Should().Be("NOT_FOUND");
    }

    // ── POST /api/todos ────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Returns201_WithLocationHeader_AndCreatedDto()
    {
        var dto = new CreateTodoDto
        {
            Title = "Integration test todo",
            Description = "Created in test",
            Priority = TodoPriority.Medium
        };

        var response = await _client.PostAsJsonAsync("/api/todos", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        envelope!.Success.Should().BeTrue();
        envelope.Data!.Title.Should().Be(dto.Title);
        envelope.Data.Priority.Should().Be(dto.Priority);
        envelope.Data.IsCompleted.Should().BeFalse();
    }

    // ── PUT /api/todos/{id} ────────────────────────────────────────────────────

    [Fact]
    public async Task Update_Returns200_WithUpdatedDto_WhenExists()
    {
        var created = await CreateTodoAsync(new CreateTodoDto { Title = "Before update" });
        var updateDto = new UpdateTodoDto
        {
            Title = "After update",
            IsCompleted = true,
            Priority = TodoPriority.Low
        };

        var response = await _client.PutAsJsonAsync($"/api/todos/{created.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        envelope!.Success.Should().BeTrue();
        envelope.Data!.Title.Should().Be("After update");
        envelope.Data.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Update_Returns404_WhenDoesNotExist()
    {
        var updateDto = new UpdateTodoDto { Title = "Doesn't matter" };

        var response = await _client.PutAsJsonAsync($"/api/todos/{Guid.NewGuid()}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        envelope!.Success.Should().BeFalse();
        envelope.ErrorCode.Should().Be("NOT_FOUND");
    }

    // ── DELETE /api/todos/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Returns204_WhenExists()
    {
        var created = await CreateTodoAsync(new CreateTodoDto { Title = "To be deleted" });

        var response = await _client.DeleteAsync($"/api/todos/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Returns404_WhenDoesNotExist()
    {
        var response = await _client.DeleteAsync($"/api/todos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<TodoResponseDto> CreateTodoAsync(CreateTodoDto dto)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", dto);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        return envelope!.Data!;
    }
}
