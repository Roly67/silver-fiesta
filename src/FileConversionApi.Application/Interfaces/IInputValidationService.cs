// <copyright file="IInputValidationService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Primitives;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Service for validating input data such as file sizes, URLs, and content types.
/// </summary>
public interface IInputValidationService
{
    /// <summary>
    /// Validates a URL against the configured allowlist/blocklist.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>A result indicating success or failure with an error message.</returns>
    Result<bool> ValidateUrl(string url);

    /// <summary>
    /// Validates that a file size is within the configured limit.
    /// </summary>
    /// <param name="sizeInBytes">The file size in bytes.</param>
    /// <returns>A result indicating success or failure with an error message.</returns>
    Result<bool> ValidateFileSize(long sizeInBytes);

    /// <summary>
    /// Validates that HTML content size is within the configured limit.
    /// </summary>
    /// <param name="content">The HTML content.</param>
    /// <returns>A result indicating success or failure with an error message.</returns>
    Result<bool> ValidateHtmlContentSize(string content);

    /// <summary>
    /// Validates that Markdown content size is within the configured limit.
    /// </summary>
    /// <param name="content">The Markdown content.</param>
    /// <returns>A result indicating success or failure with an error message.</returns>
    Result<bool> ValidateMarkdownContentSize(string content);

    /// <summary>
    /// Validates that a content type is allowed for the specified conversion type.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <param name="conversionType">The type of conversion (html, markdown, image).</param>
    /// <returns>A result indicating success or failure with an error message.</returns>
    Result<bool> ValidateContentType(string contentType, string conversionType);

    /// <summary>
    /// Gets the maximum file size in bytes.
    /// </summary>
    /// <returns>The maximum file size in bytes.</returns>
    long GetMaxFileSizeBytes();

    /// <summary>
    /// Gets the maximum HTML content size in bytes.
    /// </summary>
    /// <returns>The maximum HTML content size in bytes.</returns>
    long GetMaxHtmlContentBytes();

    /// <summary>
    /// Gets the maximum Markdown content size in bytes.
    /// </summary>
    /// <returns>The maximum Markdown content size in bytes.</returns>
    long GetMaxMarkdownContentBytes();
}
