// <copyright file="WatermarkPosition.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Watermark position on the page.
/// </summary>
public enum WatermarkPosition
{
    /// <summary>
    /// Center of the page.
    /// </summary>
    Center,

    /// <summary>
    /// Top-left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top-center.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Top-right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// Bottom-left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom-center.
    /// </summary>
    BottomCenter,

    /// <summary>
    /// Bottom-right corner.
    /// </summary>
    BottomRight,

    /// <summary>
    /// Tile across the entire page.
    /// </summary>
    Tile,
}
