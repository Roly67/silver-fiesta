// <copyright file="JobCleanupSettingsTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;

using FluentAssertions;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="JobCleanupSettings"/>.
/// </summary>
public class JobCleanupSettingsTests
{
    /// <summary>
    /// Tests that SectionName has the correct value.
    /// </summary>
    [Fact]
    public void SectionName_ShouldBeJobCleanup()
    {
        // Assert
        JobCleanupSettings.SectionName.Should().Be("JobCleanup");
    }

    /// <summary>
    /// Tests that Enabled defaults to true.
    /// </summary>
    [Fact]
    public void Enabled_ShouldDefaultToTrue()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Assert
        settings.Enabled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that RunIntervalMinutes defaults to 60.
    /// </summary>
    [Fact]
    public void RunIntervalMinutes_ShouldDefaultTo60()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Assert
        settings.RunIntervalMinutes.Should().Be(60);
    }

    /// <summary>
    /// Tests that CompletedJobRetentionDays defaults to 7.
    /// </summary>
    [Fact]
    public void CompletedJobRetentionDays_ShouldDefaultTo7()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Assert
        settings.CompletedJobRetentionDays.Should().Be(7);
    }

    /// <summary>
    /// Tests that FailedJobRetentionDays defaults to 30.
    /// </summary>
    [Fact]
    public void FailedJobRetentionDays_ShouldDefaultTo30()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Assert
        settings.FailedJobRetentionDays.Should().Be(30);
    }

    /// <summary>
    /// Tests that BatchSize defaults to 100.
    /// </summary>
    [Fact]
    public void BatchSize_ShouldDefaultTo100()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Assert
        settings.BatchSize.Should().Be(100);
    }

    /// <summary>
    /// Tests that Enabled can be set.
    /// </summary>
    [Fact]
    public void Enabled_CanBeSet()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Act
        settings.Enabled = false;

        // Assert
        settings.Enabled.Should().BeFalse();
    }

    /// <summary>
    /// Tests that RunIntervalMinutes can be set.
    /// </summary>
    [Fact]
    public void RunIntervalMinutes_CanBeSet()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Act
        settings.RunIntervalMinutes = 30;

        // Assert
        settings.RunIntervalMinutes.Should().Be(30);
    }

    /// <summary>
    /// Tests that CompletedJobRetentionDays can be set.
    /// </summary>
    [Fact]
    public void CompletedJobRetentionDays_CanBeSet()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Act
        settings.CompletedJobRetentionDays = 14;

        // Assert
        settings.CompletedJobRetentionDays.Should().Be(14);
    }

    /// <summary>
    /// Tests that FailedJobRetentionDays can be set.
    /// </summary>
    [Fact]
    public void FailedJobRetentionDays_CanBeSet()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Act
        settings.FailedJobRetentionDays = 60;

        // Assert
        settings.FailedJobRetentionDays.Should().Be(60);
    }

    /// <summary>
    /// Tests that BatchSize can be set.
    /// </summary>
    [Fact]
    public void BatchSize_CanBeSet()
    {
        // Arrange
        var settings = new JobCleanupSettings();

        // Act
        settings.BatchSize = 50;

        // Assert
        settings.BatchSize.Should().Be(50);
    }

    /// <summary>
    /// Tests that all properties can be set via object initializer.
    /// </summary>
    [Fact]
    public void ObjectInitializer_ShouldSetAllProperties()
    {
        // Arrange & Act
        var settings = new JobCleanupSettings
        {
            Enabled = false,
            RunIntervalMinutes = 15,
            CompletedJobRetentionDays = 3,
            FailedJobRetentionDays = 10,
            BatchSize = 25,
        };

        // Assert
        settings.Enabled.Should().BeFalse();
        settings.RunIntervalMinutes.Should().Be(15);
        settings.CompletedJobRetentionDays.Should().Be(3);
        settings.FailedJobRetentionDays.Should().Be(10);
        settings.BatchSize.Should().Be(25);
    }
}
