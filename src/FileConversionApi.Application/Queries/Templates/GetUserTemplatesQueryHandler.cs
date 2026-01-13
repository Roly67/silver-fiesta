// <copyright file="GetUserTemplatesQueryHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Primitives;
using FileConversionApi.Domain.ValueObjects;

using MediatR;

namespace FileConversionApi.Application.Queries.Templates;

/// <summary>
/// Handles the GetUserTemplatesQuery.
/// </summary>
public class GetUserTemplatesQueryHandler
    : IRequestHandler<GetUserTemplatesQuery, Result<IReadOnlyList<ConversionTemplateDto>>>
{
    private readonly IConversionTemplateRepository templateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserTemplatesQueryHandler"/> class.
    /// </summary>
    /// <param name="templateRepository">The template repository.</param>
    public GetUserTemplatesQueryHandler(IConversionTemplateRepository templateRepository)
    {
        this.templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<ConversionTemplateDto>>> Handle(
        GetUserTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

        var templates = string.IsNullOrWhiteSpace(request.TargetFormat)
            ? await this.templateRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false)
            : await this.templateRepository.GetByUserIdAndFormatAsync(
                userId,
                request.TargetFormat.ToLowerInvariant(),
                cancellationToken).ConfigureAwait(false);

        var dtos = templates
            .Select(ConversionTemplateDto.FromEntity)
            .ToList();

        return dtos;
    }
}
