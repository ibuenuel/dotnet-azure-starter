namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Structured error value object. Replaces plain string errors in the Result pattern,
/// enabling controllers to map error types to HTTP status codes without string comparisons.
/// </summary>
/// <param name="Code">Machine-readable identifier for the error (e.g. "NOT_FOUND").</param>
/// <param name="Message">Human-readable description for clients or logs.</param>
/// <param name="Type">Category used for HTTP status code mapping.</param>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Represents the absence of an error on a successful result.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>Creates a Not Found error.</summary>
    public static Error NotFound(string message) =>
        new("NOT_FOUND", message, ErrorType.NotFound);

    /// <summary>Creates a Validation error.</summary>
    public static Error Validation(string message) =>
        new("VALIDATION", message, ErrorType.Validation);

    /// <summary>Creates a Conflict error.</summary>
    public static Error Conflict(string message) =>
        new("CONFLICT", message, ErrorType.Conflict);

    /// <summary>Creates an Unexpected error.</summary>
    public static Error Unexpected(string message) =>
        new("UNEXPECTED", message, ErrorType.Unexpected);
}
