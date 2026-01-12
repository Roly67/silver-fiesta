// <copyright file="DatabaseHealthCheckTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.HealthChecks;
using FileConversionApi.Infrastructure.Persistence;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Moq;

namespace FileConversionApi.UnitTests.Infrastructure.HealthChecks;

/// <summary>
/// Unit tests for <see cref="DatabaseHealthCheck"/>.
/// </summary>
public class DatabaseHealthCheckTests
{
    /// <summary>
    /// Tests that constructor throws ArgumentNullException when dbContext is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenDbContextIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new DatabaseHealthCheck(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dbContext");
    }

    /// <summary>
    /// Tests that CheckHealthAsync returns Healthy when database is connected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseConnected_ReturnsHealthy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new AppDbContext(options);
        var healthCheck = new DatabaseHealthCheck(dbContext);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("database", healthCheck, null, null),
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("PostgreSQL connection successful");
    }
}
