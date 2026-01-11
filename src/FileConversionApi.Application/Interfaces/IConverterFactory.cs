// <copyright file="IConverterFactory.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Factory interface for creating file converters.
/// </summary>
public interface IConverterFactory
{
    /// <summary>
    /// Gets a converter for the specified source and target formats.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <returns>The converter if available; otherwise, null.</returns>
    IFileConverter? GetConverter(string sourceFormat, string targetFormat);

    /// <summary>
    /// Checks if a conversion is supported.
    /// </summary>
    /// <param name="sourceFormat">The source format.</param>
    /// <param name="targetFormat">The target format.</param>
    /// <returns>True if the conversion is supported; otherwise, false.</returns>
    bool IsConversionSupported(string sourceFormat, string targetFormat);
}
