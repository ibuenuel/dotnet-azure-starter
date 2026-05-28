namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Wraps a paginated collection with navigation metadata.
/// Returned by repository and service methods for all list queries.
/// </summary>
/// <typeparam name="T">Type of the items in the page.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>The items on the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; init; }

    /// <summary>Maximum items per page as requested.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Total number of pages derived from TotalCount and PageSize.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;
}
