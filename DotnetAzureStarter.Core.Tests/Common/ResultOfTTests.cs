using DotnetAzureStarter.Core.Common;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Common;

public sealed class ResultOfTTests
{
    [Fact]
    public void Success_SetsValueAndIsSuccess()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_SetsError_AndValueIsDefault()
    {
        var error = Error.NotFound("not found");

        var result = Result<int>.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Value.Should().Be(default);
    }

    [Fact]
    public void Match_OnSuccess_InvokesSuccessBranch_WithValue()
    {
        var result = Result<int>.Success(5);

        var output = result.Match(onSuccess: v => v * 2, onFailure: _ => -1);

        output.Should().Be(10);
    }

    [Fact]
    public void Match_OnFailure_InvokesFailureBranch_WithError()
    {
        var result = Result<int>.Failure(Error.Validation("bad input"));

        var output = result.Match(onSuccess: v => v, onFailure: e => -1);

        output.Should().Be(-1);
    }

    [Fact]
    public void GetValueOrDefault_ReturnsValue_WhenSuccess()
    {
        var result = Result<string>.Success("hello");

        result.GetValueOrDefault("fallback").Should().Be("hello");
    }

    [Fact]
    public void GetValueOrDefault_ReturnsFallback_WhenFailure()
    {
        var result = Result<string>.Failure(Error.NotFound("missing"));

        result.GetValueOrDefault("fallback").Should().Be("fallback");
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<int> result = 99;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }
}
