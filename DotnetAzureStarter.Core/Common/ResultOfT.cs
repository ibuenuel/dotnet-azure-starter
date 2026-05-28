namespace DotnetAzureStarter.Core.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail with a typed return value.
/// Avoids throwing exceptions for expected business failures.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Indicates whether the operation was successful.</summary>
    public bool IsSuccess { get; }

    /// <summary>Indicates whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The success value. Only meaningful when IsSuccess is true.</summary>
    public T? Value { get; }

    /// <summary>The structured error. Only meaningful when IsFailure is true.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result with the given value.</summary>
    public static Result<T> Success(T value) => new(true, value, Error.None);

    /// <summary>Creates a failed result with the given structured error.</summary>
    public static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>Implicitly converts a value to a successful result.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Returns the value if successful, or the provided default if failed.</summary>
    public T GetValueOrDefault(T defaultValue = default!)
        => IsSuccess && Value is not null ? Value : defaultValue;

    /// <summary>
    /// Executes one of two functions depending on success or failure.
    /// Enables functional-style result handling without null checks or IsSuccess branches.
    /// </summary>
    /// <typeparam name="TOut">Return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked with the value when the result is successful.</param>
    /// <param name="onFailure">Invoked with the error when the result is a failure.</param>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error);
}
