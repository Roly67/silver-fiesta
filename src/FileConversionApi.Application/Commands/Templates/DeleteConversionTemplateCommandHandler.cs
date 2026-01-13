// <copyright file="DeleteConversionTemplateCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Templates;

/// <summary>
/// Handles the DeleteConversionTemplateCommand.
/// </summary>
public class DeleteConversionTemplateCommandHandler
    : IRequestHandler<DeleteConversionTemplateCommand, Result<Unit>>
{
    private readonly IConversionTemplateRepository templateRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<DeleteConversionTemplateCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConversionTemplateCommandHandler"/> class.
    /// </summary>
    /// <param name="templateRepository">The template repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public DeleteConversionTemplateCommandHandler(
        IConversionTemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteConversionTemplateCommandHandler> logger)
    {
        this.templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> Handle(
        DeleteConversionTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var templateId = new ConversionTemplateId(request.TemplateId);
        var userId = new UserId(request.UserId);

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
            return new Error("Template.AccessDenied", "You do not have permission to delete this template.");
        }

        this.templateRepository.Delete(template);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation(
            "Deleted conversion template {TemplateId} for user {UserId}",
            template.Id.Value,
            request.UserId);

        return Unit.Value;
    }
}
