// <copyright file="UsageQuotaConfiguration.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileConversionApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for <see cref="UsageQuota"/>.
/// </summary>
public class UsageQuotaConfiguration : IEntityTypeConfiguration<UsageQuota>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UsageQuota> builder)
    {
        builder.ToTable("UsageQuotas");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasConversion(
                id => id.Value,
                value => UsageQuotaId.From(value))
            .ValueGeneratedNever();

        builder.Property(q => q.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.HasIndex(q => q.UserId);

        // Unique constraint on user + year + month
        builder.HasIndex(q => new { q.UserId, q.Year, q.Month })
            .IsUnique();

        builder.Property(q => q.Year)
            .IsRequired();

        builder.Property(q => q.Month)
            .IsRequired();

        builder.Property(q => q.ConversionsUsed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(q => q.ConversionsLimit)
            .IsRequired();

        builder.Property(q => q.BytesProcessed)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(q => q.BytesLimit)
            .IsRequired();

        builder.Property(q => q.CreatedAt)
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .IsRequired();

        // Ignore computed properties
        builder.Ignore(q => q.IsConversionsQuotaExceeded);
        builder.Ignore(q => q.IsBytesQuotaExceeded);
        builder.Ignore(q => q.IsQuotaExceeded);
        builder.Ignore(q => q.RemainingConversions);
        builder.Ignore(q => q.RemainingBytes);
    }
}
