using DotnetAzureStarter.Core.Common;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_SetsIsSuccessTrue_AndIsFailureFalse()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_SetsIsFailureTrue_AndExposesError()
    {
        var error = Error.Unexpected("Something went wrong");

        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Match_OnSuccess_InvokesSuccessBranch()
    {
        var result = Result.Success();

        var output = result.Match(onSuccess: () => "ok", onFailure: _ => "fail");

        output.Should().Be("ok");
    }

    [Fact]
    public void Match_OnFailure_InvokesFailureBranch_WithError()
    {
        var error = Error.NotFound("Item not found");
        var result = Result.Failure(error);

        var output = result.Match(onSuccess: () => "ok", onFailure: e => e.Code);

        output.Should().Be("NOT_FOUND");
    }
}
