// <copyright file="PdfWatermarkService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using Microsoft.Extensions.Logging;

using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service for adding watermarks to PDF documents using PdfSharpCore.
/// </summary>
public class PdfWatermarkService : IPdfWatermarkService
{
    private readonly ILogger<PdfWatermarkService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfWatermarkService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PdfWatermarkService(ILogger<PdfWatermarkService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<Result<byte[]>> ApplyWatermarkAsync(
        byte[] pdfData,
        WatermarkOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.Text))
            {
                return Task.FromResult<Result<byte[]>>(new Error(
                    "Watermark.EmptyText",
                    "Watermark text cannot be empty."));
            }

            this.logger.LogInformation(
                "Applying watermark to PDF with text: {WatermarkText}",
                options.Text);

            using var inputStream = new MemoryStream(pdfData);
            using var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

            var color = this.ParseColor(options.Color, options.Opacity);
            var font = new XFont(options.FontFamily, options.FontSize, XFontStyle.Bold);

            for (int i = 0; i < document.PageCount; i++)
            {
                if (!this.ShouldWatermarkPage(i + 1, options))
                {
                    continue;
                }

                var page = document.Pages[i];
                this.ApplyWatermarkToPage(page, options, font, color);
            }

            using var outputStream = new MemoryStream();
            document.Save(outputStream, false);

            this.logger.LogInformation(
                "Successfully applied watermark to {PageCount} pages",
                document.PageCount);

            return Task.FromResult<Result<byte[]>>(outputStream.ToArray());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to apply watermark to PDF");
            return Task.FromResult<Result<byte[]>>(new Error(
                "Watermark.Failed",
                $"Failed to apply watermark: {ex.Message}"));
        }
    }

    private bool ShouldWatermarkPage(int pageNumber, WatermarkOptions options)
    {
        if (options.AllPages)
        {
            return true;
        }

        return options.PageNumbers?.Contains(pageNumber) ?? false;
    }

    private void ApplyWatermarkToPage(PdfPage page, WatermarkOptions options, XFont font, XColor color)
    {
        using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

        var brush = new XSolidBrush(color);
        var textSize = gfx.MeasureString(options.Text!, font);

        if (options.Position == WatermarkPosition.Tile)
        {
            this.ApplyTiledWatermark(gfx, page, options, font, brush, textSize);
        }
        else
        {
            this.ApplySingleWatermark(gfx, page, options, font, brush, textSize);
        }
    }

    private void ApplySingleWatermark(
        XGraphics gfx,
        PdfPage page,
        WatermarkOptions options,
        XFont font,
        XBrush brush,
        XSize textSize)
    {
        var (x, y) = this.CalculatePosition(page, options.Position, textSize);

        gfx.TranslateTransform(x, y);
        gfx.RotateTransform(options.Rotation);
        gfx.DrawString(
            options.Text!,
            font,
            brush,
            new XPoint(0, 0),
            XStringFormats.Center);
    }

    private void ApplyTiledWatermark(
        XGraphics gfx,
        PdfPage page,
        WatermarkOptions options,
        XFont font,
        XBrush brush,
        XSize textSize)
    {
        var pageWidth = page.Width.Point;
        var pageHeight = page.Height.Point;

        // Calculate spacing between tiles
        var horizontalSpacing = textSize.Width * 1.5;
        var verticalSpacing = textSize.Height * 3;

        // Apply rotation to entire graphics context
        var centerX = pageWidth / 2;
        var centerY = pageHeight / 2;

        gfx.TranslateTransform(centerX, centerY);
        gfx.RotateTransform(options.Rotation);
        gfx.TranslateTransform(-centerX, -centerY);

        // Calculate extended bounds to cover rotated page
        var diagonal = Math.Sqrt((pageWidth * pageWidth) + (pageHeight * pageHeight));

        for (double y = -diagonal / 2; y < pageHeight + (diagonal / 2); y += verticalSpacing)
        {
            for (double x = -diagonal / 2; x < pageWidth + (diagonal / 2); x += horizontalSpacing)
            {
                gfx.DrawString(
                    options.Text!,
                    font,
                    brush,
                    new XPoint(x, y),
                    XStringFormats.Center);
            }
        }
    }

    private (double X, double Y) CalculatePosition(PdfPage page, WatermarkPosition position, XSize textSize)
    {
        var pageWidth = page.Width.Point;
        var pageHeight = page.Height.Point;
        var margin = 50.0;

        return position switch
        {
            WatermarkPosition.Center => (pageWidth / 2, pageHeight / 2),
            WatermarkPosition.TopLeft => (margin + (textSize.Width / 2), margin + (textSize.Height / 2)),
            WatermarkPosition.TopCenter => (pageWidth / 2, margin + (textSize.Height / 2)),
            WatermarkPosition.TopRight => (pageWidth - margin - (textSize.Width / 2), margin + (textSize.Height / 2)),
            WatermarkPosition.BottomLeft => (margin + (textSize.Width / 2), pageHeight - margin - (textSize.Height / 2)),
            WatermarkPosition.BottomCenter => (pageWidth / 2, pageHeight - margin - (textSize.Height / 2)),
            WatermarkPosition.BottomRight => (pageWidth - margin - (textSize.Width / 2), pageHeight - margin - (textSize.Height / 2)),
            _ => (pageWidth / 2, pageHeight / 2),
        };
    }

    private XColor ParseColor(string hexColor, double opacity)
    {
        try
        {
            var color = hexColor.TrimStart('#');

            if (color.Length == 6)
            {
                var r = Convert.ToInt32(color.Substring(0, 2), 16);
                var g = Convert.ToInt32(color.Substring(2, 2), 16);
                var b = Convert.ToInt32(color.Substring(4, 2), 16);
                var a = (int)(opacity * 255);

                return XColor.FromArgb(a, r, g, b);
            }
        }
        catch
        {
            // Fall back to default gray
        }

        return XColor.FromArgb((int)(opacity * 255), 128, 128, 128);
    }
}
