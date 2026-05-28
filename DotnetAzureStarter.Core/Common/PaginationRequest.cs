namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Encapsulates pagination parameters for list queries.
/// Passed into repository and service methods to request a specific page of results.
/// </summary>
/// <param name="Page">1-based page number. Defaults to 1.</param>
/// <param name="PageSize">Maximum items per page. Defaults to 20.</param>
public sealed record PaginationRequest(int Page = 1, int PageSize = 20)
{
    /// <summary>Number of items to skip to reach the requested page.</summary>
    public int Skip => (Page - 1) * PageSize;
}
