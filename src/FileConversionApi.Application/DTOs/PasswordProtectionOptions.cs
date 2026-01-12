// <copyright file="PasswordProtectionOptions.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Application.DTOs;

/// <summary>
/// Options for PDF password protection and encryption.
/// </summary>
public class PasswordProtectionOptions
{
    /// <summary>
    /// Gets or sets the user password (required to open the document).
    /// </summary>
    public string? UserPassword { get; set; }

    /// <summary>
    /// Gets or sets the owner password (required to change permissions).
    /// If not set, defaults to the user password.
    /// </summary>
    public string? OwnerPassword { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether printing is allowed.
    /// </summary>
    public bool AllowPrinting { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether copying content is allowed.
    /// </summary>
    public bool AllowCopyingContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether modifying the document is allowed.
    /// </summary>
    public bool AllowModifying { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether adding annotations is allowed.
    /// </summary>
    public bool AllowAnnotations { get; set; } = false;
}
