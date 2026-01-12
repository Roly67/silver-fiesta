// <copyright file="MarkdownToHtmlConverter.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;

using Markdig;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Infrastructure.Converters;

/// <summary>
/// Converter for Markdown to HTML using Markdig.
/// </summary>
public class MarkdownToHtmlConverter : IFileConverter
{
    private readonly ILogger<MarkdownToHtmlConverter> logger;
    private readonly MarkdownPipeline pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownToHtmlConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public MarkdownToHtmlConverter(ILogger<MarkdownToHtmlConverter> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseTaskLists()
            .UseAutoLinks()
            .Build();
    }

    /// <inheritdoc/>
    public string SourceFormat => "markdown";

    /// <inheritdoc/>
    public string TargetFormat => "html";

    /// <inheritdoc/>
    public async Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug("Starting Markdown to HTML conversion");

            using var reader = new StreamReader(input);
            var markdown = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            var htmlBody = Markdown.ToHtml(markdown, this.pipeline);
            var styledHtml = this.WrapWithStyling(htmlBody);

            this.logger.LogInformation("Markdown to HTML conversion completed successfully");

            return Encoding.UTF8.GetBytes(styledHtml);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Markdown to HTML conversion failed");
            return ConversionJobErrors.ConversionFailed(ex.Message);
        }
    }

    private static string GetDefaultStyles()
    {
        return """
            * {
                box-sizing: border-box;
            }

            body {
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Noto Sans', Helvetica, Arial, sans-serif;
                font-size: 16px;
                line-height: 1.6;
                color: #24292f;
                background-color: #ffffff;
                margin: 0;
                padding: 20px;
            }

            .markdown-body {
                max-width: 900px;
                margin: 0 auto;
            }

            h1, h2, h3, h4, h5, h6 {
                margin-top: 24px;
                margin-bottom: 16px;
                font-weight: 600;
                line-height: 1.25;
            }

            h1 {
                font-size: 2em;
                padding-bottom: 0.3em;
                border-bottom: 1px solid #d0d7de;
            }

            h2 {
                font-size: 1.5em;
                padding-bottom: 0.3em;
                border-bottom: 1px solid #d0d7de;
            }

            h3 { font-size: 1.25em; }
            h4 { font-size: 1em; }
            h5 { font-size: 0.875em; }
            h6 { font-size: 0.85em; color: #656d76; }

            p {
                margin-top: 0;
                margin-bottom: 16px;
            }

            a {
                color: #0969da;
                text-decoration: none;
            }

            a:hover {
                text-decoration: underline;
            }

            code {
                font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
                font-size: 85%;
                padding: 0.2em 0.4em;
                background-color: #f6f8fa;
                border-radius: 6px;
            }

            pre {
                font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
                font-size: 85%;
                padding: 16px;
                overflow: auto;
                line-height: 1.45;
                background-color: #f6f8fa;
                border-radius: 6px;
                margin-bottom: 16px;
            }

            pre code {
                padding: 0;
                background-color: transparent;
                border-radius: 0;
            }

            blockquote {
                margin: 0 0 16px 0;
                padding: 0 1em;
                color: #656d76;
                border-left: 0.25em solid #d0d7de;
            }

            ul, ol {
                margin-top: 0;
                margin-bottom: 16px;
                padding-left: 2em;
            }

            li {
                margin-top: 0.25em;
            }

            li + li {
                margin-top: 0.25em;
            }

            table {
                border-collapse: collapse;
                width: 100%;
                margin-bottom: 16px;
            }

            table th,
            table td {
                padding: 6px 13px;
                border: 1px solid #d0d7de;
            }

            table th {
                font-weight: 600;
                background-color: #f6f8fa;
            }

            table tr:nth-child(2n) {
                background-color: #f6f8fa;
            }

            hr {
                height: 0.25em;
                padding: 0;
                margin: 24px 0;
                background-color: #d0d7de;
                border: 0;
            }

            img {
                max-width: 100%;
                height: auto;
            }

            .task-list-item {
                list-style-type: none;
            }

            .task-list-item input[type="checkbox"] {
                margin-right: 0.5em;
            }
        """;
    }

    private string WrapWithStyling(string htmlBody)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                    {GetDefaultStyles()}
                </style>
            </head>
            <body>
                <article class="markdown-body">
                    {htmlBody}
                </article>
            </body>
            </html>
            """;
    }
}
