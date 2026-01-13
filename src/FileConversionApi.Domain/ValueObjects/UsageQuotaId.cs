// <copyright file="UsageQuotaId.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed usage quota identifier.
/// </summary>
/// <param name="Value">The underlying GUID value.</param>
public readonly record struct UsageQuotaId(Guid Value)
{
    /// <summary>
    /// Creates a new usage quota identifier.
    /// </summary>
    /// <returns>A new <see cref="UsageQuotaId"/>.</returns>
    public static UsageQuotaId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a usage quota identifier from a GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A <see cref="UsageQuotaId"/> with the specified value.</returns>
    public static UsageQuotaId From(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of the usage quota identifier.
    /// </summary>
    /// <returns>The GUID as a string.</returns>
    public override string ToString() => this.Value.ToString();
}
