// <copyright file="UsageQuota.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Domain.Entities;

/// <summary>
/// Represents a user's API usage quota for a specific month.
/// </summary>
public class UsageQuota
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsageQuota"/> class.
    /// </summary>
    /// <remarks>Required by EF Core.</remarks>
    private UsageQuota()
    {
    }

    /// <summary>
    /// Gets the unique identifier for this usage quota record.
    /// </summary>
    public UsageQuotaId Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the user this quota belongs to.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Gets the year this quota applies to.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Gets the month this quota applies to (1-12).
    /// </summary>
    public int Month { get; private set; }

    /// <summary>
    /// Gets the number of conversions used in this period.
    /// </summary>
    public int ConversionsUsed { get; private set; }

    /// <summary>
    /// Gets the maximum number of conversions allowed in this period.
    /// </summary>
    public int ConversionsLimit { get; private set; }

    /// <summary>
    /// Gets the total bytes processed in this period.
    /// </summary>
    public long BytesProcessed { get; private set; }

    /// <summary>
    /// Gets the maximum bytes allowed to be processed in this period.
    /// </summary>
    public long BytesLimit { get; private set; }

    /// <summary>
    /// Gets the date and time when this quota record was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when this quota was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the conversions quota has been exceeded.
    /// </summary>
    public bool IsConversionsQuotaExceeded => this.ConversionsUsed >= this.ConversionsLimit;

    /// <summary>
    /// Gets a value indicating whether the bytes quota has been exceeded.
    /// </summary>
    public bool IsBytesQuotaExceeded => this.BytesProcessed >= this.BytesLimit;

    /// <summary>
    /// Gets a value indicating whether any quota has been exceeded.
    /// </summary>
    public bool IsQuotaExceeded => this.IsConversionsQuotaExceeded || this.IsBytesQuotaExceeded;

    /// <summary>
    /// Gets the remaining conversions available.
    /// </summary>
    public int RemainingConversions => Math.Max(0, this.ConversionsLimit - this.ConversionsUsed);

    /// <summary>
    /// Gets the remaining bytes available.
    /// </summary>
    public long RemainingBytes => Math.Max(0, this.BytesLimit - this.BytesProcessed);

    /// <summary>
    /// Creates a new usage quota record for a user and month.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month (1-12).</param>
    /// <param name="conversionsLimit">The maximum conversions allowed.</param>
    /// <param name="bytesLimit">The maximum bytes allowed.</param>
    /// <returns>A new <see cref="UsageQuota"/> instance.</returns>
    public static UsageQuota Create(
        UserId userId,
        int year,
        int month,
        int conversionsLimit,
        long bytesLimit)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        if (conversionsLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(conversionsLimit), "Conversions limit cannot be negative.");
        }

        if (bytesLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesLimit), "Bytes limit cannot be negative.");
        }

        var now = DateTimeOffset.UtcNow;
        return new UsageQuota
        {
            Id = UsageQuotaId.New(),
            UserId = userId,
            Year = year,
            Month = month,
            ConversionsUsed = 0,
            ConversionsLimit = conversionsLimit,
            BytesProcessed = 0,
            BytesLimit = bytesLimit,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    /// Records a conversion usage.
    /// </summary>
    /// <param name="bytesProcessed">The number of bytes processed in this conversion.</param>
    public void RecordUsage(long bytesProcessed = 0)
    {
        this.ConversionsUsed++;
        this.BytesProcessed += bytesProcessed;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the quota limits.
    /// </summary>
    /// <param name="conversionsLimit">The new conversions limit.</param>
    /// <param name="bytesLimit">The new bytes limit.</param>
    public void UpdateLimits(int conversionsLimit, long bytesLimit)
    {
        if (conversionsLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(conversionsLimit), "Conversions limit cannot be negative.");
        }

        if (bytesLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesLimit), "Bytes limit cannot be negative.");
        }

        this.ConversionsLimit = conversionsLimit;
        this.BytesLimit = bytesLimit;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Resets the usage counters (typically for a new period).
    /// </summary>
    public void ResetUsage()
    {
        this.ConversionsUsed = 0;
        this.BytesProcessed = 0;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
