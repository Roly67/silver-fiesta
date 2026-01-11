// <copyright file="ConversionJobDtoTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;
using Xunit;

namespace FileConversionApi.UnitTests.Application.DTOs;

/// <summary>
/// Unit tests for <see cref="ConversionJobDto"/>.
/// </summary>
public class ConversionJobDtoTests
{
    /// <summary>
    /// Tests that FromEntity correctly maps Id from entity.
    /// </summary>
    [Fact]
    public void FromEntity_WhenCalled_MapsIdCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Id.Should().Be(job.Id.Value);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps SourceFormat from entity.
    /// </summary>
    [Fact]
    public void FromEntity_WhenCalled_MapsSourceFormatCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "HTML", "pdf", "test.html");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.SourceFormat.Should().Be("html"); // Should be lowercase
    }

    /// <summary>
    /// Tests that FromEntity correctly maps TargetFormat from entity.
    /// </summary>
    [Fact]
    public void FromEntity_WhenCalled_MapsTargetFormatCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "PDF", "test.html");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.TargetFormat.Should().Be("pdf"); // Should be lowercase
    }

    /// <summary>
    /// Tests that FromEntity correctly maps InputFileName from entity.
    /// </summary>
    [Fact]
    public void FromEntity_WhenCalled_MapsInputFileNameCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var inputFileName = "my-document.html";
        var job = ConversionJob.Create(userId, "html", "pdf", inputFileName);

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.InputFileName.Should().Be(inputFileName);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps Status for pending job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsPending_MapsStatusCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Status.Should().Be(ConversionStatus.Pending);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps Status for processing job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsProcessing_MapsStatusCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Status.Should().Be(ConversionStatus.Processing);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps Status for completed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsCompleted_MapsStatusCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 });

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Status.Should().Be(ConversionStatus.Completed);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps Status for failed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobHasFailed_MapsStatusCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        job.MarkAsFailed("Conversion error");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Status.Should().Be(ConversionStatus.Failed);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps OutputFileName for completed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsCompleted_MapsOutputFileNameCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var outputFileName = "converted-document.pdf";
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted(outputFileName, new byte[] { 0x25, 0x50, 0x44, 0x46 });

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.OutputFileName.Should().Be(outputFileName);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps OutputFileName as null for pending job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsPending_MapsOutputFileNameAsNull()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.OutputFileName.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromEntity correctly maps ErrorMessage for failed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobHasFailed_MapsErrorMessageCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var errorMessage = "Conversion failed due to invalid HTML format";
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        job.MarkAsFailed(errorMessage);

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.ErrorMessage.Should().Be(errorMessage);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps ErrorMessage as null for successful job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsCompleted_MapsErrorMessageAsNull()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        job.MarkAsCompleted("output.pdf", new byte[] { 0x25 });

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromEntity correctly maps CreatedAt from entity.
    /// </summary>
    [Fact]
    public void FromEntity_WhenCalled_MapsCreatedAtCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var beforeCreation = DateTimeOffset.UtcNow;
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        var afterCreation = DateTimeOffset.UtcNow;

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        dto.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps CompletedAt for completed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsCompleted_MapsCompletedAtCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        var beforeCompletion = DateTimeOffset.UtcNow;
        job.MarkAsCompleted("output.pdf", new byte[] { 0x25 });
        var afterCompletion = DateTimeOffset.UtcNow;

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.CompletedAt.Should().NotBeNull();
        dto.CompletedAt!.Value.Should().BeOnOrAfter(beforeCompletion);
        dto.CompletedAt.Value.Should().BeOnOrBefore(afterCompletion);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps CompletedAt for failed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobHasFailed_MapsCompletedAtCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();
        var beforeFailure = DateTimeOffset.UtcNow;
        job.MarkAsFailed("Error");
        var afterFailure = DateTimeOffset.UtcNow;

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.CompletedAt.Should().NotBeNull();
        dto.CompletedAt!.Value.Should().BeOnOrAfter(beforeFailure);
        dto.CompletedAt.Value.Should().BeOnOrBefore(afterFailure);
    }

    /// <summary>
    /// Tests that FromEntity correctly maps CompletedAt as null for pending job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsPending_MapsCompletedAtAsNull()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.CompletedAt.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromEntity correctly maps CompletedAt as null for processing job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsProcessing_MapsCompletedAtAsNull()
    {
        // Arrange
        var userId = UserId.New();
        var job = ConversionJob.Create(userId, "html", "pdf", "test.html");
        job.MarkAsProcessing();

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.CompletedAt.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromEntity maps all properties correctly for a complete job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobIsComplete_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var inputFileName = "document.html";
        var outputFileName = "document.pdf";
        var outputData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D };
        var job = ConversionJob.Create(userId, "html", "pdf", inputFileName);
        job.MarkAsProcessing();
        job.MarkAsCompleted(outputFileName, outputData);

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Id.Should().Be(job.Id.Value);
        dto.SourceFormat.Should().Be("html");
        dto.TargetFormat.Should().Be("pdf");
        dto.Status.Should().Be(ConversionStatus.Completed);
        dto.InputFileName.Should().Be(inputFileName);
        dto.OutputFileName.Should().Be(outputFileName);
        dto.ErrorMessage.Should().BeNull();
        dto.CreatedAt.Should().Be(job.CreatedAt);
        dto.CompletedAt.Should().Be(job.CompletedAt);
    }

    /// <summary>
    /// Tests that FromEntity maps all properties correctly for a failed job.
    /// </summary>
    [Fact]
    public void FromEntity_WhenJobHasFailed_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var userId = UserId.New();
        var inputFileName = "document.html";
        var errorMessage = "Invalid HTML content";
        var job = ConversionJob.Create(userId, "html", "pdf", inputFileName);
        job.MarkAsProcessing();
        job.MarkAsFailed(errorMessage);

        // Act
        var dto = ConversionJobDto.FromEntity(job);

        // Assert
        dto.Id.Should().Be(job.Id.Value);
        dto.SourceFormat.Should().Be("html");
        dto.TargetFormat.Should().Be("pdf");
        dto.Status.Should().Be(ConversionStatus.Failed);
        dto.InputFileName.Should().Be(inputFileName);
        dto.OutputFileName.Should().BeNull();
        dto.ErrorMessage.Should().Be(errorMessage);
        dto.CreatedAt.Should().Be(job.CreatedAt);
        dto.CompletedAt.Should().Be(job.CompletedAt);
    }
}
