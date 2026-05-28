namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Standardised API response envelope. Every endpoint returns this wrapper
/// to guarantee a consistent response shape regardless of success or failure.
/// </summary>
/// <typeparam name="T">Type of the response payload.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>Indicates whether the request was handled successfully.</summary>
    public bool Success { get; init; }

    /// <summary>The response payload. Null on failure.</summary>
    public T? Data { get; init; }

    /// <summary>Machine-readable error code for client-side error handling.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Human-readable error message. Null on success.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Correlation ID for distributed tracing and log correlation.</summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>Creates a successful response wrapping the given data.</summary>
    public static ApiResponse<T> Ok(T data, string traceId = "") =>
        new()
        {
            Success = true,
            Data = data,
            TraceId = traceId
        };

    /// <summary>Creates a failed response from a structured error.</summary>
    public static ApiResponse<T> Fail(Error error, string traceId = "") =>
        new()
        {
            Success = false,
            ErrorCode = error.Code,
            ErrorMessage = error.Message,
            TraceId = traceId
        };
}
