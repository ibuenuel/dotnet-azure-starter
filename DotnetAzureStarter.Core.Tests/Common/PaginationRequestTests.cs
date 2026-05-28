using DotnetAzureStarter.Core.Common;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Common;

public sealed class PaginationRequestTests
{
    [Fact]
    public void Defaults_ArePage1_AndPageSize20()
    {
        var request = new PaginationRequest();

        request.Page.Should().Be(1);
        request.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 10, 20)]
    [InlineData(5, 5, 20)]
    public void Skip_IsComputedCorrectly(int page, int pageSize, int expectedSkip)
    {
        var request = new PaginationRequest(page, pageSize);

        request.Skip.Should().Be(expectedSkip);
    }
}
