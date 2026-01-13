// <copyright file="ConversionTemplateId.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed conversion template identifier.
/// </summary>
/// <param name="Value">The underlying GUID value.</param>
public readonly record struct ConversionTemplateId(Guid Value)
{
    /// <summary>
    /// Creates a new conversion template identifier.
    /// </summary>
    /// <returns>A new <see cref="ConversionTemplateId"/>.</returns>
    public static ConversionTemplateId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a conversion template identifier from a GUID.
    /// </summary>
    /// <param name="value">The GUID value.</param>
    /// <returns>A <see cref="ConversionTemplateId"/> with the specified value.</returns>
    public static ConversionTemplateId From(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of the conversion template identifier.
    /// </summary>
    /// <returns>The GUID as a string.</returns>
    public override string ToString() => this.Value.ToString();
}
