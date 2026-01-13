// <copyright file="CloudStorageServiceTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileConversionApi.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for the <see cref="CloudStorageService"/> class.
/// </summary>
public class CloudStorageServiceTests : IDisposable
{
    private readonly Mock<ILogger<CloudStorageService>> loggerMock;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageServiceTests"/> class.
    /// </summary>
    public CloudStorageServiceTests()
    {
        this.loggerMock = new Mock<ILogger<CloudStorageService>>();
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CloudStorageService(null!, this.loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var settings = Options.Create(new CloudStorageSettings());

        // Act
        var act = () => new CloudStorageService(settings, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that IsEnabled returns false when storage is disabled.
    /// </summary>
    [Fact]
    public void IsEnabled_WhenStorageIsDisabled_ReturnsFalse()
    {
        // Arrange
        var settings = new CloudStorageSettings { Enabled = false };
        using var service = this.CreateService(settings);

        // Act
        var isEnabled = service.IsEnabled;

        // Assert
        isEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsEnabled returns true when storage is enabled.
    /// </summary>
    [Fact]
    public void IsEnabled_WhenStorageIsEnabled_ReturnsTrue()
    {
        // Arrange
        var settings = new CloudStorageSettings
        {
            Enabled = true,
            ServiceUrl = "http://localhost:9000",
            BucketName = "test-bucket",
            AccessKey = "test-access",
            SecretKey = "test-secret",
        };
        using var service = this.CreateService(settings);

        // Act
        var isEnabled = service.IsEnabled;

        // Assert
        isEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that UploadAsync returns success with disabled storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UploadAsync_WhenStorageIsDisabled_ReturnsFailure()
    {
        // Arrange
        var settings = new CloudStorageSettings { Enabled = false };
        using var service = this.CreateService(settings);
        var data = new byte[] { 0x00, 0x01, 0x02 };

        // Act
        var result = await service.UploadAsync(data, "test-key", "application/octet-stream", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CloudStorage.Disabled");
    }

    /// <summary>
    /// Tests that DownloadAsync returns failure when storage is disabled.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DownloadAsync_WhenStorageIsDisabled_ReturnsFailure()
    {
        // Arrange
        var settings = new CloudStorageSettings { Enabled = false };
        using var service = this.CreateService(settings);

        // Act
        var result = await service.DownloadAsync("test-key", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CloudStorage.Disabled");
    }

    /// <summary>
    /// Tests that DeleteAsync returns failure when storage is disabled.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DeleteAsync_WhenStorageIsDisabled_ReturnsFailure()
    {
        // Arrange
        var settings = new CloudStorageSettings { Enabled = false };
        using var service = this.CreateService(settings);

        // Act
        var result = await service.DeleteAsync("test-key", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CloudStorage.Disabled");
    }

    /// <summary>
    /// Tests that service can be created with valid settings.
    /// </summary>
    [Fact]
    public void CreateService_WithValidSettings_DoesNotThrow()
    {
        // Arrange
        var settings = new CloudStorageSettings
        {
            Enabled = true,
            ServiceUrl = "http://localhost:9000",
            BucketName = "test-bucket",
            AccessKey = "minioadmin",
            SecretKey = "minioadmin",
            Region = "us-east-1",
            ForcePathStyle = true,
        };

        // Act
        var act = () =>
        {
            using var service = this.CreateService(settings);
        };

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times.
    /// </summary>
    [Fact]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var settings = new CloudStorageSettings
        {
            Enabled = true,
            ServiceUrl = "http://localhost:9000",
            BucketName = "test-bucket",
            AccessKey = "minioadmin",
            SecretKey = "minioadmin",
        };
        var service = this.CreateService(settings);

        // Act
        var act = () =>
        {
            service.Dispose();
            service.Dispose();
            service.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            this.disposed = true;
        }
    }

    private CloudStorageService CreateService(CloudStorageSettings settings)
    {
        var options = Options.Create(settings);
        return new CloudStorageService(options, this.loggerMock.Object);
    }
}
