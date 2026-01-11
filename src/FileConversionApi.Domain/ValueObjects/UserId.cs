// <copyright file="UserId.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed user identifier.
/// </summary>
/// <param name="Value">The underlying GUID value.</param>
public readonly record struct UserId(Guid Value)
{
    /// <summary>
    /// Creates a new user identifier.
    /// </summary>
    /// <returns>A new <see cref="UserId"/>.</returns>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a user identifier from a GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A <see cref="UserId"/> with the specified value.</returns>
    public static UserId From(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of the user identifier.
    /// </summary>
    /// <returns>The GUID as a string.</returns>
    public override string ToString() => this.Value.ToString();
}
