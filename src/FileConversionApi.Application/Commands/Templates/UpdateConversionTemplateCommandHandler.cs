// <copyright file="UpdateConversionTemplateCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Text.Json;

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Templates;

/// <summary>
/// Handles the UpdateConversionTemplateCommand.
/// </summary>
public class UpdateConversionTemplateCommandHandler
    : IRequestHandler<UpdateConversionTemplateCommand, Result<ConversionTemplateDto>>
{
    private static readonly string[] ValidTargetFormats = ["pdf", "html", "png", "jpeg", "webp", "gif", "bmp"];

    private readonly IConversionTemplateRepository templateRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<UpdateConversionTemplateCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateConversionTemplateCommandHandler"/> class.
    /// </summary>
    /// <param name="templateRepository">The template repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public UpdateConversionTemplateCommandHandler(
        IConversionTemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateConversionTemplateCommandHandler> logger)
    {
        this.templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionTemplateDto>> Handle(
        UpdateConversionTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var templateId = new ConversionTemplateId(request.TemplateId);
        var userId = new UserId(request.UserId);
        var targetFormat = request.TargetFormat.ToLowerInvariant();

        // Get the template
        var template = await this.templateRepository
            .GetByIdAsync(templateId, cancellationToken)
            .ConfigureAwait(false);

        if (template is null)
        {
            return new Error("Template.NotFound", $"Template with ID '{request.TemplateId}' was not found.");
        }

        // Verify ownership
        if (template.UserId != userId)
        {
            return new Error("Template.AccessDenied", "You do not have permission to modify this template.");
        }

        // Validate target format
        if (!ValidTargetFormats.Contains(targetFormat))
        {
            return new Error(
                "Template.InvalidTargetFormat",
                $"Invalid target format '{request.TargetFormat}'. Valid formats: {string.Join(", ", ValidTargetFormats)}.");
        }

        // Check if new name conflicts with existing template (excluding current)
        var nameExists = await this.templateRepository
            .NameExistsForUserAsync(userId, request.Name, templateId, cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            return new Error(
                "Template.NameAlreadyExists",
                $"A template with the name '{request.Name}' already exists.");
        }

        // Update the template
        template.UpdateDetails(request.Name, request.Description);
        template.UpdateTargetFormat(targetFormat);

        var optionsJson = request.Options != null
            ? JsonSerializer.Serialize(request.Options, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            : "{}";
        template.UpdateOptions(optionsJson);

        this.templateRepository.Update(template);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation(
            "Updated conversion template {TemplateId} for user {UserId}",
            template.Id.Value,
            request.UserId);

        return ConversionTemplateDto.FromEntity(template);
    }
}
