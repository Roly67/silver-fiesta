// <copyright file="GetTemplateByIdQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

namespace FileConversionApi.Application.Queries.Templates;

/// <summary>
/// Handles the GetTemplateByIdQuery.
/// </summary>
public class GetTemplateByIdQueryHandler
    : IRequestHandler<GetTemplateByIdQuery, Result<ConversionTemplateDto>>
{
    private readonly IConversionTemplateRepository templateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTemplateByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="templateRepository">The template repository.</param>
    public GetTemplateByIdQueryHandler(IConversionTemplateRepository templateRepository)
    {
        this.templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<ConversionTemplateDto>> Handle(
        GetTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var templateId = new ConversionTemplateId(request.TemplateId);
        var userId = new UserId(request.UserId);

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
            return new Error("Template.AccessDenied", "You do not have permission to view this template.");
        }

        return ConversionTemplateDto.FromEntity(template);
    }
}
