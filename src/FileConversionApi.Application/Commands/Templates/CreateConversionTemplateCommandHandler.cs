// <copyright file="CreateConversionTemplateCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text.Json;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Templates;

/// <summary>
/// Handles the CreateConversionTemplateCommand.
/// </summary>
public class CreateConversionTemplateCommandHandler
    : IRequestHandler<CreateConversionTemplateCommand, Result<ConversionTemplateDto>>
{
    private static readonly string[] ValidTargetFormats = ["pdf", "html", "png", "jpeg", "webp", "gif", "bmp"];

    private readonly IConversionTemplateRepository templateRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<CreateConversionTemplateCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConversionTemplateCommandHandler"/> class.
    /// </summary>
    /// <param name="templateRepository">The template repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public CreateConversionTemplateCommandHandler(
        IConversionTemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateConversionTemplateCommandHandler> logger)
    {
        this.templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionTemplateDto>> Handle(
        CreateConversionTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var targetFormat = request.TargetFormat.ToLowerInvariant();

        // Validate target format
        if (!ValidTargetFormats.Contains(targetFormat))
        {
            return new Error(
                "Template.InvalidTargetFormat",
                $"Invalid target format '{request.TargetFormat}'. Valid formats: {string.Join(", ", ValidTargetFormats)}.");
        }

        // Check if template name already exists for this user
        var nameExists = await this.templateRepository
            .NameExistsForUserAsync(userId, request.Name, cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            return new Error(
                "Template.NameAlreadyExists",
                $"A template with the name '{request.Name}' already exists.");
        }

        // Serialize options to JSON
        var optionsJson = request.Options != null
            ? JsonSerializer.Serialize(request.Options, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            : "{}";

        // Create the template
        var template = ConversionTemplate.Create(
            userId,
            request.Name,
            targetFormat,
            optionsJson,
            request.Description);

        await this.templateRepository.AddAsync(template, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation(
            "Created conversion template {TemplateId} for user {UserId}",
            template.Id.Value,
            request.UserId);

        return ConversionTemplateDto.FromEntity(template);
    }
}
