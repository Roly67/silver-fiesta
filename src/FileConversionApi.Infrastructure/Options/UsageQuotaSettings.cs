// <copyright file="UsageQuotaSettings.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Infrastructure.Options;

/// <summary>
/// Configuration settings for API usage quotas.
/// </summary>
public class UsageQuotaSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "UsageQuotas";

    /// <summary>
    /// Gets or sets a value indicating whether usage quotas are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default monthly conversions limit for new users.
    /// </summary>
    public int DefaultMonthlyConversions { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the default monthly bytes limit for new users (default: 1GB).
    /// </summary>
    public long DefaultMonthlyBytes { get; set; } = 1L * 1024 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the monthly conversions limit for admin users.
    /// A value of 0 means unlimited.
    /// </summary>
    public int AdminMonthlyConversions { get; set; } = 0;

    /// <summary>
    /// Gets or sets the monthly bytes limit for admin users (default: unlimited).
    /// A value of 0 means unlimited.
    /// </summary>
    public long AdminMonthlyBytes { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether to exempt admin users from quotas.
    /// </summary>
    public bool ExemptAdmins { get; set; } = true;
}
