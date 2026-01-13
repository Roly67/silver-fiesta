// <copyright file="UserRateLimitService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Collections.Concurrent;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Service implementation for managing user rate limit settings.
/// </summary>
public class UserRateLimitService : IUserRateLimitService
{
    private readonly IUserRateLimitSettingsRepository settingsRepository;
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMemoryCache cache;
    private readonly RateLimitingSettings settings;
    private readonly ILogger<UserRateLimitService> logger;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> loadLocks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRateLimitService"/> class.
    /// </summary>
    /// <param name="settingsRepository">The settings repository.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="cache">The memory cache.</param>
    /// <param name="settings">The rate limiting settings.</param>
    /// <param name="logger">The logger.</param>
    public UserRateLimitService(
        IUserRateLimitSettingsRepository settingsRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        IOptions<RateLimitingSettings> settings,
        ILogger<UserRateLimitService> logger)
    {
        this.settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<UserEffectiveRateLimits> GetEffectiveLimitsAsync(
        UserId userId,
        string policyName,
        CancellationToken cancellationToken)
    {
        // Check admin bypass
        if (await this.ShouldBypassRateLimitAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            return new UserEffectiveRateLimits
            {
                BypassRateLimiting = true,
                PermitLimit = int.MaxValue,
                Window = TimeSpan.FromHours(1),
                Source = "Admin",
            };
        }

        // Get cached or load user settings
        var userSettings = await this.GetCachedOrLoadSettingsAsync(userId, cancellationToken).ConfigureAwait(false);
        var tier = userSettings?.Tier ?? RateLimitTier.Free;

        // Check for per-user overrides
        var (overrideLimit, overrideWindow) = policyName.ToLowerInvariant() switch
        {
            "standard" => (userSettings?.StandardPolicyPermitLimit, userSettings?.StandardPolicyWindowMinutes),
            "conversion" => (userSettings?.ConversionPolicyPermitLimit, userSettings?.ConversionPolicyWindowMinutes),
            _ => (null, null),
        };

        // Get tier defaults
        var tierSettings = this.settings.GetTierSettings(tier);
        var policySettings = policyName.ToLowerInvariant() switch
        {
            "standard" => tierSettings.StandardPolicy,
            "conversion" => tierSettings.ConversionPolicy,
            _ => tierSettings.StandardPolicy,
        };

        // Use override values if set, otherwise use tier defaults
        if (overrideLimit.HasValue || overrideWindow.HasValue)
        {
            return new UserEffectiveRateLimits
            {
                BypassRateLimiting = false,
                PermitLimit = overrideLimit ?? policySettings.PermitLimit,
                Window = TimeSpan.FromMinutes(overrideWindow ?? policySettings.WindowMinutes),
                Source = "Override",
            };
        }

        // Check for Unlimited tier
        if (tier == RateLimitTier.Unlimited)
        {
            return new UserEffectiveRateLimits
            {
                BypassRateLimiting = true,
                PermitLimit = policySettings.PermitLimit,
                Window = TimeSpan.FromMinutes(policySettings.WindowMinutes),
                Source = "Tier",
            };
        }

        return new UserEffectiveRateLimits
        {
            BypassRateLimiting = false,
            PermitLimit = policySettings.PermitLimit,
            Window = TimeSpan.FromMinutes(policySettings.WindowMinutes),
            Source = "Tier",
        };
    }

    /// <inheritdoc/>
    public async Task<bool> ShouldBypassRateLimitAsync(UserId userId, CancellationToken cancellationToken)
    {
        if (!this.settings.ExemptAdmins)
        {
            return false;
        }

        var user = await this.userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return user?.IsAdmin == true;
    }

    /// <inheritdoc/>
    public async Task<UserRateLimitSettings> GetOrCreateSettingsAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var existingSettings = await this.settingsRepository.GetByUserIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (existingSettings is not null)
        {
            return existingSettings;
        }

        var newSettings = UserRateLimitSettings.Create(userId);
        await this.settingsRepository.AddAsync(newSettings, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation(
            "Created new rate limit settings for user {UserId} with tier {Tier}",
            userId.Value,
            newSettings.Tier);

        // Cache the new settings
        this.CacheSettings(userId, newSettings);

        return newSettings;
    }

    /// <inheritdoc/>
    public async Task<Result<UserRateLimitSettings>> GetSettingsAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var userSettings = await this.GetCachedOrLoadSettingsAsync(userId, cancellationToken).ConfigureAwait(false);

        if (userSettings is null)
        {
            return RateLimitErrors.SettingsNotFound(userId);
        }

        return Result<UserRateLimitSettings>.Success(userSettings);
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateTierAsync(
        UserId userId,
        RateLimitTier tier,
        CancellationToken cancellationToken)
    {
        var userSettings = await this.GetOrCreateSettingsAsync(userId, cancellationToken).ConfigureAwait(false);
        userSettings.UpdateTier(tier);
        this.settingsRepository.Update(userSettings);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.InvalidateCache(userId);

        this.logger.LogInformation(
            "Updated rate limit tier for user {UserId} to {Tier}",
            userId.Value,
            tier);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> SetPolicyOverrideAsync(
        UserId userId,
        string policyName,
        int? permitLimit,
        int? windowMinutes,
        CancellationToken cancellationToken)
    {
        var normalizedPolicy = policyName.ToLowerInvariant();
        if (normalizedPolicy != "standard" && normalizedPolicy != "conversion")
        {
            return RateLimitErrors.InvalidPolicyName(policyName);
        }

        var userSettings = await this.GetOrCreateSettingsAsync(userId, cancellationToken).ConfigureAwait(false);

        if (normalizedPolicy == "standard")
        {
            userSettings.SetStandardPolicyOverride(permitLimit, windowMinutes);
        }
        else
        {
            userSettings.SetConversionPolicyOverride(permitLimit, windowMinutes);
        }

        this.settingsRepository.Update(userSettings);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.InvalidateCache(userId);

        this.logger.LogInformation(
            "Set {Policy} policy override for user {UserId}: PermitLimit={PermitLimit}, WindowMinutes={WindowMinutes}",
            policyName,
            userId.Value,
            permitLimit,
            windowMinutes);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> ClearOverridesAsync(UserId userId, CancellationToken cancellationToken)
    {
        var userSettings = await this.settingsRepository.GetByUserIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (userSettings is null)
        {
            return RateLimitErrors.SettingsNotFound(userId);
        }

        userSettings.ClearAllOverrides();
        this.settingsRepository.Update(userSettings);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.InvalidateCache(userId);

        this.logger.LogInformation("Cleared all rate limit overrides for user {UserId}", userId.Value);

        return Result.Success();
    }

    /// <inheritdoc/>
    public void InvalidateCache(UserId userId)
    {
        var cacheKey = GetCacheKey(userId);
        this.cache.Remove(cacheKey);
        this.logger.LogDebug("Invalidated rate limit cache for user {UserId}", userId.Value);
    }

    private static string GetCacheKey(UserId userId) => $"RateLimitSettings:{userId.Value}";

    private async Task<UserRateLimitSettings?> GetCachedOrLoadSettingsAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(userId);

        if (this.cache.TryGetValue(cacheKey, out UserRateLimitSettings? cachedSettings))
        {
            return cachedSettings;
        }

        // Use per-user lock to prevent thundering herd
        var loadLock = this.loadLocks.GetOrAdd(userId.Value, _ => new SemaphoreSlim(1, 1));

        await loadLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock
            if (this.cache.TryGetValue(cacheKey, out cachedSettings))
            {
                return cachedSettings;
            }

            // Load from database
            var settings = await this.settingsRepository.GetByUserIdAsync(userId, cancellationToken)
                .ConfigureAwait(false);

            if (settings is not null)
            {
                this.CacheSettings(userId, settings);
            }

            return settings;
        }
        finally
        {
            loadLock.Release();
        }
    }

    private void CacheSettings(UserId userId, UserRateLimitSettings userSettings)
    {
        var cacheKey = GetCacheKey(userId);
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(this.settings.UserSettingsCacheSeconds))
            .SetSlidingExpiration(TimeSpan.FromSeconds(this.settings.UserSettingsCacheSeconds / 2));

        this.cache.Set(cacheKey, userSettings, cacheOptions);
    }
}
