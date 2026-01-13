// <copyright file="UserRateLimitSettingsConfiguration.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileConversionApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for <see cref="UserRateLimitSettings"/>.
/// </summary>
public class UserRateLimitSettingsConfiguration : IEntityTypeConfiguration<UserRateLimitSettings>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserRateLimitSettings> builder)
    {
        builder.ToTable("UserRateLimitSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => UserRateLimitSettingsId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        // Unique constraint on UserId (one settings record per user)
        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.Property(s => s.Tier)
            .IsRequired()
            .HasDefaultValue(RateLimitTier.Free);

        builder.Property(s => s.StandardPolicyPermitLimit)
            .IsRequired(false);

        builder.Property(s => s.StandardPolicyWindowMinutes)
            .IsRequired(false);

        builder.Property(s => s.ConversionPolicyPermitLimit)
            .IsRequired(false);

        builder.Property(s => s.ConversionPolicyWindowMinutes)
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // Ignore computed properties
        builder.Ignore(s => s.HasStandardPolicyOverride);
        builder.Ignore(s => s.HasConversionPolicyOverride);
        builder.Ignore(s => s.HasAnyOverride);
    }
}
