// <copyright file="UsageQuotaService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service implementation for managing usage quotas.
/// </summary>
public class UsageQuotaService : IUsageQuotaService
{
    private readonly IUsageQuotaRepository quotaRepository;
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly UsageQuotaSettings settings;
    private readonly ILogger<UsageQuotaService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageQuotaService"/> class.
    /// </summary>
    /// <param name="quotaRepository">The quota repository.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="settings">The quota settings.</param>
    /// <param name="logger">The logger.</param>
    public UsageQuotaService(
        IUsageQuotaRepository quotaRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IOptions<UsageQuotaSettings> settings,
        ILogger<UsageQuotaService> logger)
    {
        this.quotaRepository = quotaRepository ?? throw new ArgumentNullException(nameof(quotaRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result> CheckQuotaAsync(UserId userId, CancellationToken cancellationToken)
    {
        if (!this.settings.Enabled)
        {
            return Result.Success();
        }

        // Check if user is admin and admins are exempt
        if (this.settings.ExemptAdmins)
        {
            var user = await this.userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            if (user?.IsAdmin == true)
            {
                return Result.Success();
            }
        }

        var quota = await this.GetOrCreateCurrentQuotaAsync(userId, cancellationToken).ConfigureAwait(false);

        if (quota.IsConversionsQuotaExceeded)
        {
            this.logger.LogWarning(
                "User {UserId} has exceeded conversions quota: {Used}/{Limit}",
                userId.Value,
                quota.ConversionsUsed,
                quota.ConversionsLimit);
            return QuotaErrors.ConversionsQuotaExceeded(quota.ConversionsUsed, quota.ConversionsLimit);
        }

        if (quota.IsBytesQuotaExceeded)
        {
            this.logger.LogWarning(
                "User {UserId} has exceeded bytes quota: {Used}/{Limit}",
                userId.Value,
                quota.BytesProcessed,
                quota.BytesLimit);
            return QuotaErrors.BytesQuotaExceeded(quota.BytesProcessed, quota.BytesLimit);
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task RecordUsageAsync(UserId userId, long bytesProcessed, CancellationToken cancellationToken)
    {
        if (!this.settings.Enabled)
        {
            return;
        }

        var quota = await this.GetOrCreateCurrentQuotaAsync(userId, cancellationToken).ConfigureAwait(false);
        quota.RecordUsage(bytesProcessed);
        this.quotaRepository.Update(quota);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogDebug(
            "Recorded usage for user {UserId}: {ConversionsUsed}/{ConversionsLimit} conversions, {BytesUsed}/{BytesLimit} bytes",
            userId.Value,
            quota.ConversionsUsed,
            quota.ConversionsLimit,
            quota.BytesProcessed,
            quota.BytesLimit);
    }

    /// <inheritdoc/>
    public async Task<Result<UsageQuota>> GetCurrentQuotaAsync(UserId userId, CancellationToken cancellationToken)
    {
        var quota = await this.GetOrCreateCurrentQuotaAsync(userId, cancellationToken).ConfigureAwait(false);
        return Result<UsageQuota>.Success(quota);
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<UsageQuota>>> GetQuotaHistoryAsync(
        UserId userId,
        int months,
        CancellationToken cancellationToken)
    {
        var allQuotas = await this.quotaRepository.GetByUserAsync(userId, cancellationToken).ConfigureAwait(false);

        // Filter to only return the requested number of months, ordered by most recent first
        IReadOnlyList<UsageQuota> filteredQuotas = allQuotas
            .OrderByDescending(q => q.Year)
            .ThenByDescending(q => q.Month)
            .Take(months)
            .ToList();

        return Result<IReadOnlyList<UsageQuota>>.Success(filteredQuotas);
    }

    /// <inheritdoc/>
    public async Task<Result<UsageQuota>> UpdateQuotaLimitsAsync(
        UserId userId,
        int conversionsLimit,
        long bytesLimit,
        CancellationToken cancellationToken)
    {
        var quota = await this.GetOrCreateCurrentQuotaAsync(userId, cancellationToken).ConfigureAwait(false);
        quota.UpdateLimits(conversionsLimit, bytesLimit);
        this.quotaRepository.Update(quota);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation(
            "Updated quota limits for user {UserId}: {ConversionsLimit} conversions, {BytesLimit} bytes",
            userId.Value,
            conversionsLimit,
            bytesLimit);

        return Result<UsageQuota>.Success(quota);
    }

    private async Task<UsageQuota> GetOrCreateCurrentQuotaAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var quota = await this.quotaRepository.GetByUserAndMonthAsync(userId, year, month, cancellationToken)
            .ConfigureAwait(false);

        if (quota is not null)
        {
            return quota;
        }

        // Determine limits based on user type
        var conversionsLimit = this.settings.DefaultMonthlyConversions;
        var bytesLimit = this.settings.DefaultMonthlyBytes;

        if (this.settings.ExemptAdmins)
        {
            var user = await this.userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            if (user?.IsAdmin == true)
            {
                conversionsLimit = this.settings.AdminMonthlyConversions;
                bytesLimit = this.settings.AdminMonthlyBytes;
            }
        }

        quota = UsageQuota.Create(userId, year, month, conversionsLimit, bytesLimit);
        await this.quotaRepository.AddAsync(quota, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation(
            "Created new quota record for user {UserId} for {Year}-{Month:D2}",
            userId.Value,
            year,
            month);

        return quota;
    }
}
