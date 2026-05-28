namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Represents the result of a void operation that can succeed or fail.
/// Use for operations without a return value (e.g. Delete, Send).
/// </summary>
public sealed class Result
{
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Indicates whether the operation was successful.</summary>
    public bool IsSuccess { get; }

    /// <summary>Indicates whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The structured error. Only meaningful when IsFailure is true.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result with the given structured error.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Executes one of two functions depending on success or failure.
    /// Enables functional-style result handling without null checks.
    /// </summary>
    /// <typeparam name="TOut">Return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked when the result is successful.</param>
    /// <param name="onFailure">Invoked with the error when the result is a failure.</param>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);
}
