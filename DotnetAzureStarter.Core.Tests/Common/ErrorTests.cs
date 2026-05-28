using DotnetAzureStarter.Core.Common;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Common;

public sealed class ErrorTests
{
    [Fact]
    public void None_HasEmptyCodeAndMessage_AndNoneType()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
        Error.None.Type.Should().Be(ErrorType.None);
    }

    [Fact]
    public void NotFound_SetsCorrectCodeAndType()
    {
        var error = Error.NotFound("Item was not found");

        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Be("Item was not found");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Validation_SetsCorrectCodeAndType()
    {
        var error = Error.Validation("Field is required");

        error.Code.Should().Be("VALIDATION");
        error.Message.Should().Be("Field is required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Conflict_SetsCorrectCodeAndType()
    {
        var error = Error.Conflict("Resource already exists");

        error.Code.Should().Be("CONFLICT");
        error.Message.Should().Be("Resource already exists");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Unexpected_SetsCorrectCodeAndType()
    {
        var error = Error.Unexpected("Internal error");

        error.Code.Should().Be("UNEXPECTED");
        error.Message.Should().Be("Internal error");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void RecordEquality_TwoIdenticalErrors_AreEqual()
    {
        var a = Error.NotFound("same message");
        var b = Error.NotFound("same message");

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentMessages_AreNotEqual()
    {
        var a = Error.NotFound("message A");
        var b = Error.NotFound("message B");

        a.Should().NotBe(b);
    }
}
