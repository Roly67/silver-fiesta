// <copyright file="ConversionJobId.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed conversion job identifier.
/// </summary>
/// <param name="Value">The underlying GUID value.</param>
public readonly record struct ConversionJobId(Guid Value)
{
    /// <summary>
    /// Creates a new conversion job identifier.
    /// </summary>
    /// <returns>A new <see cref="ConversionJobId"/>.</returns>
    public static ConversionJobId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a conversion job identifier from a GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A <see cref="ConversionJobId"/> with the specified value.</returns>
    public static ConversionJobId From(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of the conversion job identifier.
    /// </summary>
    /// <returns>The GUID as a string.</returns>
    public override string ToString() => this.Value.ToString();
}
