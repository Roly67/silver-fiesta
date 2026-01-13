// <copyright file="XlsxToPdfConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for XLSX (Excel) spreadsheets to PDF format using LibreOffice.
/// </summary>
public class XlsxToPdfConverter : OfficeConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XlsxToPdfConverter"/> class.
    /// </summary>
    /// <param name="libreOfficeService">The LibreOffice service.</param>
    /// <param name="watermarkService">The watermark service.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="logger">The logger.</param>
    public XlsxToPdfConverter(
        ILibreOfficeService libreOfficeService,
        IPdfWatermarkService watermarkService,
        IPdfEncryptionService encryptionService,
        ILogger<XlsxToPdfConverter> logger)
        : base(libreOfficeService, watermarkService, encryptionService, logger)
    {
    }

    /// <inheritdoc/>
    public override string SourceFormat => "xlsx";
}
