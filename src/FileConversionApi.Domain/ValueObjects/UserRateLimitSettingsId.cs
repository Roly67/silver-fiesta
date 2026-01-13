// <copyright file="UserRateLimitSettingsId.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed user rate limit settings identifier.
/// </summary>
/// <param name="Value">The underlying GUID value.</param>
public readonly record struct UserRateLimitSettingsId(Guid Value)
{
    /// <summary>
    /// Creates a new user rate limit settings identifier.
    /// </summary>
    /// <returns>A new <see cref="UserRateLimitSettingsId"/>.</returns>
    public static UserRateLimitSettingsId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a user rate limit settings identifier from a GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A <see cref="UserRateLimitSettingsId"/> with the specified value.</returns>
    public static UserRateLimitSettingsId From(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of the user rate limit settings identifier.
    /// </summary>
    /// <returns>The GUID as a string.</returns>
    public override string ToString() => this.Value.ToString();
}
