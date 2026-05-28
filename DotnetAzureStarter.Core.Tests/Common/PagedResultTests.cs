using DotnetAzureStarter.Core.Common;
using FluentAssertions;

namespace DotnetAzureStarter.Core.Tests.Common;

public sealed class PagedResultTests
{
    [Theory]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(0, 20, 0)]
    [InlineData(40, 20, 2)]
    [InlineData(41, 20, 3)]
    public void TotalPages_IsComputedCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        var result = new PagedResult<int> { TotalCount = totalCount, PageSize = pageSize, Page = 1 };

        result.TotalPages.Should().Be(expectedPages);
    }

    [Fact]
    public void HasNextPage_IsTrue_WhenNotOnLastPage()
    {
        var result = new PagedResult<int> { Page = 1, PageSize = 10, TotalCount = 25 };

        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_IsFalse_WhenOnLastPage()
    {
        var result = new PagedResult<int> { Page = 3, PageSize = 10, TotalCount = 25 };

        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_IsTrue_WhenNotOnFirstPage()
    {
        var result = new PagedResult<int> { Page = 2, PageSize = 10, TotalCount = 25 };

        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasPreviousPage_IsFalse_WhenOnFirstPage()
    {
        var result = new PagedResult<int> { Page = 1, PageSize = 10, TotalCount = 25 };

        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void SinglePage_HasNeither_NextNorPreviousPage()
    {
        var result = new PagedResult<int> { Page = 1, PageSize = 20, TotalCount = 5 };

        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }
}
