using DotnetAzureStarter.Core.Common;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Common;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_SetsSuccessTrue_AndPopulatesData()
    {
        var response = ApiResponse<string>.Ok("payload");

        response.Success.Should().BeTrue();
        response.Data.Should().Be("payload");
        response.ErrorCode.Should().BeNull();
        response.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Ok_WithTraceId_SetsTraceId()
    {
        var response = ApiResponse<int>.Ok(1, "trace-123");

        response.TraceId.Should().Be("trace-123");
    }

    [Fact]
    public void Fail_SetsSuccessFalse_AndPopulatesErrorFields()
    {
        var error = Error.NotFound("Item not found");

        var response = ApiResponse<string>.Fail(error);

        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
        response.ErrorCode.Should().Be("NOT_FOUND");
        response.ErrorMessage.Should().Be("Item not found");
    }

    [Fact]
    public void Fail_WithTraceId_SetsTraceId()
    {
        var error = Error.Validation("bad");
        var response = ApiResponse<string>.Fail(error, "trace-abc");

        response.TraceId.Should().Be("trace-abc");
    }
}
