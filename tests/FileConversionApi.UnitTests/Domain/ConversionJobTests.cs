// <copyright file="ConversionJobTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;

using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Enums;
using FileConversionApi.Domain.ValueObjects;

using FluentAssertions;

using Xunit;

namespace FileConversionApi.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="ConversionJob"/> entity.
/// </summary>
public class ConversionJobTests
{
    private const string ValidSourceFormat = "PDF";
    private const string ValidTargetFormat = "DOCX";
    private const string ValidInputFileName = "document.pdf";

    private readonly UserId validUserId = UserId.New();

    /// <summary>
    /// Tests that Create returns a job with a valid ID.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithValidId()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.Id.Value.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that Create returns a job with the specified user ID.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithSpecifiedUserId()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.UserId.Should().Be(this.validUserId);
    }

    /// <summary>
    /// Tests that Create returns a job with source format in lowercase.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithSourceFormatInLowercase()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            "PDF",
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.SourceFormat.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that Create returns a job with target format in lowercase.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithTargetFormatInLowercase()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            "DOCX",
            ValidInputFileName);

        // Assert
        job.TargetFormat.Should().Be("docx");
    }

    /// <summary>
    /// Tests that Create returns a job with the specified input file name.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithSpecifiedInputFileName()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.InputFileName.Should().Be(ValidInputFileName);
    }

    /// <summary>
    /// Tests that Create returns a job with Pending status.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithPendingStatus()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.Status.Should().Be(ConversionStatus.Pending);
    }

    /// <summary>
    /// Tests that Create sets CreatedAt to current UTC time.
    /// </summary>
    [Fact]
    public void Create_ShouldSetCreatedAtToCurrentUtcTime()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        var after = DateTimeOffset.UtcNow;
        job.CreatedAt.Should().BeOnOrAfter(before);
        job.CreatedAt.Should().BeOnOrBefore(after);
    }

    /// <summary>
    /// Tests that Create returns a job with null OutputFileName.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithNullOutputFileName()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.OutputFileName.Should().BeNull();
    }

    /// <summary>
    /// Tests that Create returns a job with null OutputData.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithNullOutputData()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.OutputData.Should().BeNull();
    }

    /// <summary>
    /// Tests that Create returns a job with null ErrorMessage.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithNullErrorMessage()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests that Create returns a job with null CompletedAt.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithNullCompletedAt()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.CompletedAt.Should().BeNull();
    }

    /// <summary>
    /// Tests that MarkAsProcessing changes status to Processing.
    /// </summary>
    [Fact]
    public void MarkAsProcessing_ShouldChangeStatusToProcessing()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Act
        job.MarkAsProcessing();

        // Assert
        job.Status.Should().Be(ConversionStatus.Processing);
    }

    /// <summary>
    /// Tests that MarkAsCompleted changes status to Completed.
    /// </summary>
    [Fact]
    public void MarkAsCompleted_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();

        // Act
        job.MarkAsCompleted("output.docx", new byte[] { 1, 2, 3 });

        // Assert
        job.Status.Should().Be(ConversionStatus.Completed);
    }

    /// <summary>
    /// Tests that MarkAsCompleted sets the output file name.
    /// </summary>
    [Fact]
    public void MarkAsCompleted_ShouldSetOutputFileName()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();
        var outputFileName = "output.docx";

        // Act
        job.MarkAsCompleted(outputFileName, new byte[] { 1, 2, 3 });

        // Assert
        job.OutputFileName.Should().Be(outputFileName);
    }

    /// <summary>
    /// Tests that MarkAsCompleted sets the output data.
    /// </summary>
    [Fact]
    public void MarkAsCompleted_ShouldSetOutputData()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();
        var outputData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        job.MarkAsCompleted("output.docx", outputData);

        // Assert
        job.OutputData.Should().BeEquivalentTo(outputData);
    }

    /// <summary>
    /// Tests that MarkAsCompleted sets CompletedAt.
    /// </summary>
    [Fact]
    public void MarkAsCompleted_ShouldSetCompletedAt()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();
        var before = DateTimeOffset.UtcNow;

        // Act
        job.MarkAsCompleted("output.docx", new byte[] { 1, 2, 3 });

        // Assert
        var after = DateTimeOffset.UtcNow;
        job.CompletedAt.Should().NotBeNull();
        job.CompletedAt!.Value.Should().BeOnOrAfter(before);
        job.CompletedAt!.Value.Should().BeOnOrBefore(after);
    }

    /// <summary>
    /// Tests that MarkAsFailed changes status to Failed.
    /// </summary>
    [Fact]
    public void MarkAsFailed_ShouldChangeStatusToFailed()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();

        // Act
        job.MarkAsFailed("Conversion failed");

        // Assert
        job.Status.Should().Be(ConversionStatus.Failed);
    }

    /// <summary>
    /// Tests that MarkAsFailed sets the error message.
    /// </summary>
    [Fact]
    public void MarkAsFailed_ShouldSetErrorMessage()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();
        var errorMessage = "File format not supported";

        // Act
        job.MarkAsFailed(errorMessage);

        // Assert
        job.ErrorMessage.Should().Be(errorMessage);
    }

    /// <summary>
    /// Tests that MarkAsFailed sets CompletedAt.
    /// </summary>
    [Fact]
    public void MarkAsFailed_ShouldSetCompletedAt()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        job.MarkAsProcessing();
        var before = DateTimeOffset.UtcNow;

        // Act
        job.MarkAsFailed("Error occurred");

        // Assert
        var after = DateTimeOffset.UtcNow;
        job.CompletedAt.Should().NotBeNull();
        job.CompletedAt!.Value.Should().BeOnOrAfter(before);
        job.CompletedAt!.Value.Should().BeOnOrBefore(after);
    }

    /// <summary>
    /// Tests full state transition from Pending to Processing to Completed.
    /// </summary>
    [Fact]
    public void StateTransition_PendingToProcessingToCompleted_ShouldSucceed()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert initial state
        job.Status.Should().Be(ConversionStatus.Pending);

        // Act - transition to Processing
        job.MarkAsProcessing();
        job.Status.Should().Be(ConversionStatus.Processing);

        // Act - transition to Completed
        job.MarkAsCompleted("output.docx", new byte[] { 1 });
        job.Status.Should().Be(ConversionStatus.Completed);
    }

    /// <summary>
    /// Tests full state transition from Pending to Processing to Failed.
    /// </summary>
    [Fact]
    public void StateTransition_PendingToProcessingToFailed_ShouldSucceed()
    {
        // Arrange
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert initial state
        job.Status.Should().Be(ConversionStatus.Pending);

        // Act - transition to Processing
        job.MarkAsProcessing();
        job.Status.Should().Be(ConversionStatus.Processing);

        // Act - transition to Failed
        job.MarkAsFailed("Error");
        job.Status.Should().Be(ConversionStatus.Failed);
    }

    /// <summary>
    /// Tests that multiple jobs have unique IDs.
    /// </summary>
    [Fact]
    public void Create_MultipleJobs_ShouldHaveUniqueIds()
    {
        // Act
        var job1 = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);
        var job2 = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job1.Id.Should().NotBe(job2.Id);
    }

    /// <summary>
    /// Tests that source format handles mixed case.
    /// </summary>
    [Fact]
    public void Create_SourceFormatMixedCase_ShouldConvertToLowercase()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            "PdF",
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.SourceFormat.Should().Be("pdf");
    }

    /// <summary>
    /// Tests that target format handles mixed case.
    /// </summary>
    [Fact]
    public void Create_TargetFormatMixedCase_ShouldConvertToLowercase()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            "DoCx",
            ValidInputFileName);

        // Assert
        job.TargetFormat.Should().Be("docx");
    }

    /// <summary>
    /// Tests that User navigation property is initially null.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnJobWithNullUser()
    {
        // Act
        var job = ConversionJob.Create(
            this.validUserId,
            ValidSourceFormat,
            ValidTargetFormat,
            ValidInputFileName);

        // Assert
        job.User.Should().BeNull();
    }
}
