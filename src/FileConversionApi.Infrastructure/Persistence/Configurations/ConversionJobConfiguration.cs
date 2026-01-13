// <copyright file="ConversionJobConfiguration.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileConversionApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for <see cref="ConversionJob"/>.
/// </summary>
public class ConversionJobConfiguration : IEntityTypeConfiguration<ConversionJob>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ConversionJob> builder)
    {
        builder.ToTable("ConversionJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasConversion(
                id => id.Value,
                value => ConversionJobId.From(value))
            .ValueGeneratedNever();

        builder.Property(j => j.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.Property(j => j.SourceFormat)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.TargetFormat)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(j => j.InputFileName)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(j => j.OutputFileName)
            .HasMaxLength(512);

        builder.Property(j => j.OutputData);

        builder.Property(j => j.ErrorMessage)
            .HasMaxLength(2048);

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.CompletedAt);

        builder.Property(j => j.WebhookUrl)
            .HasMaxLength(2048);

        builder.Property(j => j.StorageLocation)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StorageLocation.Database);

        builder.Property(j => j.CloudStorageKey)
            .HasMaxLength(1024);

        builder.HasIndex(j => j.UserId);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.CreatedAt);
        builder.HasIndex(j => j.StorageLocation);
    }
}
