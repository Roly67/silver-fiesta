// <copyright file="ConversionTemplateConfiguration.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileConversionApi.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for <see cref="ConversionTemplate"/>.
/// </summary>
public class ConversionTemplateConfiguration : IEntityTypeConfiguration<ConversionTemplate>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ConversionTemplate> builder)
    {
        builder.ToTable("ConversionTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => ConversionTemplateId.From(value))
            .ValueGeneratedNever();

        builder.Property(t => t.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .IsRequired();

        builder.HasIndex(t => t.UserId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => new { t.UserId, t.Name })
            .IsUnique();

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.TargetFormat)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(t => new { t.UserId, t.TargetFormat });

        builder.Property(t => t.OptionsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);
    }
}
