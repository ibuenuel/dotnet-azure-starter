namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Categorises error types to enable deterministic HTTP status code mapping in controllers.
/// </summary>
public enum ErrorType
{
    /// <summary>No error — used as the default on successful results.</summary>
    None,

    /// <summary>Requested resource does not exist → 404 Not Found.</summary>
    NotFound,

    /// <summary>Input data failed validation rules → 422 Unprocessable Entity.</summary>
    Validation,

    /// <summary>Operation conflicts with current resource state → 409 Conflict.</summary>
    Conflict,

    /// <summary>An unexpected internal error occurred → 500 Internal Server Error.</summary>
    Unexpected
}
