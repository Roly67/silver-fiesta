// <copyright file="QuotaCheckBehaviorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Application.Behaviors;
using FileConversionApi.Application.Commands.Conversion;
using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Xunit;

namespace FileConversionApi.UnitTests.Application.Behaviors;

/// <summary>
/// Unit tests for <see cref="QuotaCheckBehavior{TRequest, TResponse}"/>.
/// </summary>
public class QuotaCheckBehaviorTests
{
    private readonly Mock<IUsageQuotaService> quotaServiceMock;
    private readonly Mock<ICurrentUserService> currentUserServiceMock;
    private readonly ILogger<QuotaCheckBehavior<ConvertHtmlToPdfCommand, Result<ConversionJobDto>>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotaCheckBehaviorTests"/> class.
    /// </summary>
    public QuotaCheckBehaviorTests()
    {
        this.quotaServiceMock = new Mock<IUsageQuotaService>();
        this.currentUserServiceMock = new Mock<ICurrentUserService>();
        this.logger = NullLogger<QuotaCheckBehavior<ConvertHtmlToPdfCommand, Result<ConversionJobDto>>>.Instance;
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when quota service is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenQuotaServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QuotaCheckBehavior<ConvertHtmlToPdfCommand, Result<ConversionJobDto>>(
            null!,
            this.currentUserServiceMock.Object,
            this.logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("quotaService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when current user service is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenCurrentUserServiceIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QuotaCheckBehavior<ConvertHtmlToPdfCommand, Result<ConversionJobDto>>(
            this.quotaServiceMock.Object,
            null!,
            this.logger);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("currentUserService");
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new QuotaCheckBehavior<ConvertHtmlToPdfCommand, Result<ConversionJobDto>>(
            this.quotaServiceMock.Object,
            this.currentUserServiceMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    /// <summary>
    /// Tests that constructor creates behavior instance with valid dependencies.
    /// </summary>
    [Fact]
    public void Constructor_WithValidDependencies_CreatesBehavior()
    {
        // Act
        var behavior = new QuotaCheckBehavior<ConvertHtmlToPdfCommand, Result<ConversionJobDto>>(
            this.quotaServiceMock.Object,
            this.currentUserServiceMock.Object,
            this.logger);

        // Assert
        behavior.Should().NotBeNull();
    }
}
