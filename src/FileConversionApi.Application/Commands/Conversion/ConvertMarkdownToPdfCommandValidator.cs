// <copyright file="ConvertMarkdownToPdfCommandValidator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

using FluentValidation;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Validator for the convert Markdown to PDF command.
/// </summary>
public class ConvertMarkdownToPdfCommandValidator : AbstractValidator<ConvertMarkdownToPdfCommand>
{
    private readonly IInputValidationService? validationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMarkdownToPdfCommandValidator"/> class.
    /// </summary>
    public ConvertMarkdownToPdfCommandValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMarkdownToPdfCommandValidator"/> class.
    /// </summary>
    /// <param name="validationService">The input validation service.</param>
    public ConvertMarkdownToPdfCommandValidator(IInputValidationService? validationService)
    {
        this.validationService = validationService;

        this.RuleFor(x => x.Markdown)
            .NotEmpty()
            .WithMessage("Markdown content is required.");

        this.RuleFor(x => x.Markdown)
            .Must(this.BeWithinMarkdownSizeLimit)
            .WithMessage(x => $"Markdown content must not exceed {this.GetMaxMarkdownSizeMb():F0}MB.");

        this.When(x => x.Options is not null, () =>
        {
            this.RuleFor(x => x.Options!.PageSize)
                .Must(BeValidPageSize)
                .WithMessage("PageSize must be A4, Letter, Legal, Tabloid, Ledger, A3, or A5.");

            this.RuleFor(x => x.Options!.JavaScriptTimeout)
                .InclusiveBetween(1000, 120000)
                .WithMessage("JavaScriptTimeout must be between 1000 and 120000 milliseconds.");
        });
    }

    private static bool BeValidPageSize(string? pageSize)
    {
        if (string.IsNullOrWhiteSpace(pageSize))
        {
            return true;
        }

        var validSizes = new[] { "A4", "Letter", "Legal", "Tabloid", "Ledger", "A3", "A5" };
        return validSizes.Contains(pageSize, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeWithinMarkdownSizeLimit(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return true;
        }

        if (this.validationService is null)
        {
            // Default limit of 5MB
            return content.Length <= 5 * 1024 * 1024;
        }

        var result = this.validationService.ValidateMarkdownContentSize(content);
        return result.IsSuccess;
    }

    private double GetMaxMarkdownSizeMb()
    {
        if (this.validationService is null)
        {
            return 5;
        }

        return this.validationService.GetMaxMarkdownContentBytes() / (1024.0 * 1024.0);
    }
}
