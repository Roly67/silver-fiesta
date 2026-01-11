// <copyright file="ConvertMarkdownToPdfCommandValidator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FluentValidation;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Validator for the convert Markdown to PDF command.
/// </summary>
public class ConvertMarkdownToPdfCommandValidator : AbstractValidator<ConvertMarkdownToPdfCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertMarkdownToPdfCommandValidator"/> class.
    /// </summary>
    public ConvertMarkdownToPdfCommandValidator()
    {
        this.RuleFor(x => x.Markdown)
            .NotEmpty()
            .WithMessage("Markdown content is required.");

        this.RuleFor(x => x.Markdown)
            .MaximumLength(10 * 1024 * 1024)
            .WithMessage("Markdown content must not exceed 10MB.");

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
}
