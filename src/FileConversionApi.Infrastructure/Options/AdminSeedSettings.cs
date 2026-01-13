// <copyright file="AdminSeedSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for seeding the default admin user.
/// </summary>
public class AdminSeedSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AdminSeed";

    /// <summary>
    /// Gets or sets a value indicating whether admin seeding is enabled.
    /// Default is true for development, should be false in production after initial setup.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default admin email address.
    /// </summary>
    public string Email { get; set; } = "admin@fileconversionapi.local";

    /// <summary>
    /// Gets or sets the default admin password.
    /// IMPORTANT: Change this in production or use environment variables.
    /// </summary>
    public string Password { get; set; } = "Admin123!";

    /// <summary>
    /// Gets or sets a value indicating whether to skip seeding if any admin user already exists.
    /// When true, seeding only occurs if there are no admin users in the database.
    /// </summary>
    public bool SkipIfAdminExists { get; set; } = true;
}
