// <copyright file="DocxToPdfConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for DOCX (Word) documents to PDF format using LibreOffice.
/// </summary>
public class DocxToPdfConverter : OfficeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocxToPdfConverter"/> class.
    /// </summary>
    /// <param name="libreOfficeService">The LibreOffice service.</param>
    /// <param name="watermarkService">The watermark service.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="logger">The logger.</param>
    public DocxToPdfConverter(
        ILibreOfficeService libreOfficeService,
        IPdfWatermarkService watermarkService,
        IPdfEncryptionService encryptionService,
        ILogger<DocxToPdfConverter> logger)
        : base(libreOfficeService, watermarkService, encryptionService, logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "docx";
}
