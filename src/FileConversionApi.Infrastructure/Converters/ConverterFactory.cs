// <copyright file="ConverterFactory.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Factory for creating file converters.
/// </summary>
public class ConverterFactory : IConverterFactory
{
    private readonly IEnumerable<IFileConverter> converters;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConverterFactory"/> class.
    /// </summary>
    /// <param name="converters">The available converters.</param>
    public ConverterFactory(IEnumerable<IFileConverter> converters)
    {
        this.converters = converters ?? throw new ArgumentNullException(nameof(converters));
    }

    /// <inheritdoc/>
    public IFileConverter? GetConverter(string sourceFormat, string targetFormat)
    {
        var normalizedSource = sourceFormat.ToLowerInvariant();
        var normalizedTarget = targetFormat.ToLowerInvariant();

        return this.converters.FirstOrDefault(c =>
            c.SourceFormat.Equals(normalizedSource, StringComparison.OrdinalIgnoreCase) &&
            c.TargetFormat.Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public bool IsConversionSupported(string sourceFormat, string targetFormat)
    {
        return this.GetConverter(sourceFormat, targetFormat) is not null;
    }
}
