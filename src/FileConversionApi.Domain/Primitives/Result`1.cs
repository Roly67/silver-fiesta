// <copyright file="Result`1.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Primitives;

/// <summary>
/// Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error if the operation failed.</param>
    internal Result(TValue value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        this.value = value;
    }

    /// <summary>
    /// Gets the value if the operation succeeded.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value of a failed result.</exception>
    public TValue Value => this.IsSuccess
        ? this.value
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Implicitly converts a value to a success result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicitly converts an error to a failure result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
