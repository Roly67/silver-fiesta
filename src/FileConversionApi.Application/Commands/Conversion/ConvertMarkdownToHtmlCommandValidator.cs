// <copyright file="ConvertMarkdownToHtmlCommandValidator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

using FluentValidation;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Validator for the convert Markdown to HTML command.
/// </summary>
public class ConvertMarkdownToHtmlCommandValidator : AbstractValidator<ConvertMarkdownToHtmlCommand>
{
    private readonly IInputValidationService? validationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMarkdownToHtmlCommandValidator"/> class.
    /// </summary>
    public ConvertMarkdownToHtmlCommandValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMarkdownToHtmlCommandValidator"/> class.
    /// </summary>
    /// <param name="validationService">The input validation service.</param>
    public ConvertMarkdownToHtmlCommandValidator(IInputValidationService? validationService)
    {
        this.validationService = validationService;

        this.RuleFor(x => x.Markdown)
            .NotEmpty()
            .WithMessage("Markdown content is required.");

        this.RuleFor(x => x.Markdown)
            .Must(this.BeWithinMarkdownSizeLimit)
            .WithMessage(x => $"Markdown content must not exceed {this.GetMaxMarkdownSizeMb():F0}MB.");
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
