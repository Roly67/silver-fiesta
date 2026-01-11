// <copyright file="IFileConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Interface for file format converters.
/// </summary>
public interface IFileConverter
{
    /// <summary>
    /// Gets the source format this converter handles.
    /// </summary>
    string SourceFormat { get; }

    /// <summary>
    /// Gets the target format this converter produces.
    /// </summary>
    string TargetFormat { get; }

    /// <summary>
    /// Converts the input to the target format.
    /// </summary>
    /// <param name="input">The input stream or content.</param>
    /// <param name="options">The conversion options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the converted bytes or an error.</returns>
    Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken);
}
