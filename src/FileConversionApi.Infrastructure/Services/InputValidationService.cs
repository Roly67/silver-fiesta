// <copyright file="InputValidationService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service for validating input data such as file sizes, URLs, and content types.
/// </summary>
public class InputValidationService : IInputValidationService
{
    private readonly InputValidationSettings settings;
    private readonly ILogger<InputValidationService> logger;
    private readonly List<Regex> allowlistPatterns;
    private readonly List<Regex> blocklistPatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputValidationService"/> class.
    /// </summary>
    /// <param name="settings">The input validation settings.</param>
    /// <param name="logger">The logger.</param>
    public InputValidationService(
        IOptions<InputValidationSettings> settings,
        ILogger<InputValidationService> logger)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Pre-compile regex patterns for better performance
        this.allowlistPatterns = CompilePatterns(this.settings.UrlValidation.Allowlist);
        this.blocklistPatterns = CompilePatterns(this.settings.UrlValidation.Blocklist);
    }

    /// <inheritdoc/>
    public Result<bool> ValidateUrl(string url)
    {
        if (!this.settings.Enabled || !this.settings.UrlValidation.Enabled)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return true; // Empty URLs are handled elsewhere
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return Error.Validation("InputValidation.InvalidUrl", "The URL is not a valid absolute URL.");
        }

        var host = uri.Host;

        // Check for private IP addresses if enabled
        if (this.settings.UrlValidation.BlockPrivateIpAddresses && IsPrivateIpAddress(host))
        {
            this.logger.LogWarning("URL validation failed: Private IP address blocked - {Host}", host);
            return Error.Validation(
                "InputValidation.PrivateIpBlocked",
                "URLs pointing to private or internal IP addresses are not allowed.");
        }

        if (this.settings.UrlValidation.UseAllowlist)
        {
            // Allowlist mode: URL must match at least one pattern
            if (!MatchesAnyPattern(host, this.allowlistPatterns))
            {
                this.logger.LogWarning("URL validation failed: Host not in allowlist - {Host}", host);
                return Error.Validation(
                    "InputValidation.UrlNotAllowed",
                    $"The URL host '{host}' is not in the allowed list.");
            }
        }
        else
        {
            // Blocklist mode: URL must not match any pattern
            if (MatchesAnyPattern(host, this.blocklistPatterns))
            {
                this.logger.LogWarning("URL validation failed: Host in blocklist - {Host}", host);
                return Error.Validation(
                    "InputValidation.UrlBlocked",
                    $"The URL host '{host}' is blocked.");
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public Result<bool> ValidateFileSize(long sizeInBytes)
    {
        if (!this.settings.Enabled)
        {
            return true;
        }

        if (sizeInBytes > this.settings.MaxFileSizeBytes)
        {
            var maxSizeMb = this.settings.MaxFileSizeBytes / (1024.0 * 1024.0);
            var actualSizeMb = sizeInBytes / (1024.0 * 1024.0);
            return Error.Validation(
                "InputValidation.FileTooLarge",
                $"File size ({actualSizeMb:F2}MB) exceeds the maximum allowed size ({maxSizeMb:F2}MB).");
        }

        return true;
    }

    /// <inheritdoc/>
    public Result<bool> ValidateHtmlContentSize(string content)
    {
        if (!this.settings.Enabled || string.IsNullOrEmpty(content))
        {
            return true;
        }

        var sizeInBytes = System.Text.Encoding.UTF8.GetByteCount(content);
        if (sizeInBytes > this.settings.MaxHtmlContentBytes)
        {
            var maxSizeMb = this.settings.MaxHtmlContentBytes / (1024.0 * 1024.0);
            var actualSizeMb = sizeInBytes / (1024.0 * 1024.0);
            return Error.Validation(
                "InputValidation.HtmlContentTooLarge",
                $"HTML content size ({actualSizeMb:F2}MB) exceeds the maximum allowed size ({maxSizeMb:F2}MB).");
        }

        return true;
    }

    /// <inheritdoc/>
    public Result<bool> ValidateMarkdownContentSize(string content)
    {
        if (!this.settings.Enabled || string.IsNullOrEmpty(content))
        {
            return true;
        }

        var sizeInBytes = System.Text.Encoding.UTF8.GetByteCount(content);
        if (sizeInBytes > this.settings.MaxMarkdownContentBytes)
        {
            var maxSizeMb = this.settings.MaxMarkdownContentBytes / (1024.0 * 1024.0);
            var actualSizeMb = sizeInBytes / (1024.0 * 1024.0);
            return Error.Validation(
                "InputValidation.MarkdownContentTooLarge",
                $"Markdown content size ({actualSizeMb:F2}MB) exceeds the maximum allowed size ({maxSizeMb:F2}MB).");
        }

        return true;
    }

    /// <inheritdoc/>
    public Result<bool> ValidateContentType(string contentType, string conversionType)
    {
        if (!this.settings.Enabled || !this.settings.ContentTypeValidation.Enabled)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return true; // Empty content types are handled elsewhere
        }

        // Extract the media type without parameters (e.g., "text/html; charset=utf-8" -> "text/html")
        var mediaType = contentType.Split(';')[0].Trim().ToLowerInvariant();

        var allowedTypes = conversionType.ToLowerInvariant() switch
        {
            "html" => this.settings.ContentTypeValidation.AllowedHtmlContentTypes,
            "markdown" or "md" => this.settings.ContentTypeValidation.AllowedMarkdownContentTypes,
            "image" => this.settings.ContentTypeValidation.AllowedImageContentTypes,
            _ => null,
        };

        if (allowedTypes is null)
        {
            return true; // Unknown conversion type, skip validation
        }

        if (!allowedTypes.Any(t => t.Equals(mediaType, StringComparison.OrdinalIgnoreCase)))
        {
            var errorMessage = $"Content type '{mediaType}' is not allowed for {conversionType} conversion. Allowed types: {string.Join(", ", allowedTypes)}.";
            return Error.Validation("InputValidation.InvalidContentType", errorMessage);
        }

        return true;
    }

    /// <inheritdoc/>
    public long GetMaxFileSizeBytes() => this.settings.MaxFileSizeBytes;

    /// <inheritdoc/>
    public long GetMaxHtmlContentBytes() => this.settings.MaxHtmlContentBytes;

    /// <inheritdoc/>
    public long GetMaxMarkdownContentBytes() => this.settings.MaxMarkdownContentBytes;

    private static List<Regex> CompilePatterns(List<string> patterns)
    {
        var regexList = new List<Regex>();
        foreach (var pattern in patterns)
        {
            // Convert wildcard patterns to regex
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            regexList.Add(new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        return regexList;
    }

    private static bool MatchesAnyPattern(string host, List<Regex> patterns)
    {
        return patterns.Any(pattern => pattern.IsMatch(host));
    }

    private static bool IsPrivateIpAddress(string host)
    {
        // Try to parse as IP address
        if (!IPAddress.TryParse(host, out var ipAddress))
        {
            // Not an IP address, try to resolve it
            try
            {
                var addresses = Dns.GetHostAddresses(host);
                return addresses.Any(IsPrivateIp);
            }
            catch
            {
                // If DNS resolution fails, we can't determine if it's private
                // In strict mode, we might want to block unknown hosts
                return false;
            }
        }

        return IsPrivateIp(ipAddress);
    }

    private static bool IsPrivateIp(IPAddress ipAddress)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Check for IPv6 loopback and link-local addresses
            return IPAddress.IsLoopback(ipAddress) ||
                   ipAddress.IsIPv6LinkLocal ||
                   ipAddress.IsIPv6SiteLocal;
        }

        // IPv4
        var bytes = ipAddress.GetAddressBytes();

        return bytes[0] switch
        {
            10 => true,   // 10.0.0.0/8
            127 => true,  // 127.0.0.0/8 (loopback)
            172 => bytes[1] >= 16 && bytes[1] <= 31,  // 172.16.0.0/12
            192 => bytes[1] == 168,  // 192.168.0.0/16
            169 => bytes[1] == 254,  // 169.254.0.0/16 (link-local)
            0 => bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0,  // 0.0.0.0
            _ => false,
        };
    }
}
