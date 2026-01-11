// <copyright file="ConvertHtmlToPdfCommandValidator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FluentValidation;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Validator for the convert HTML to PDF command.
/// </summary>
public class ConvertHtmlToPdfCommandValidator : AbstractValidator<ConvertHtmlToPdfCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToPdfCommandValidator"/> class.
    /// </summary>
    public ConvertHtmlToPdfCommandValidator()
    {
        this.RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.HtmlContent) || !string.IsNullOrWhiteSpace(x.Url))
            .WithMessage("Either HtmlContent or Url must be provided.");

        this.When(x => !string.IsNullOrWhiteSpace(x.Url), () =>
        {
            this.RuleFor(x => x.Url)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Url must be a valid HTTP or HTTPS URL.");
        });

        this.When(x => !string.IsNullOrWhiteSpace(x.HtmlContent), () =>
        {
            this.RuleFor(x => x.HtmlContent)
                .MaximumLength(10 * 1024 * 1024)
                .WithMessage("HtmlContent must not exceed 10MB.");
        });

        this.When(x => x.Options is not null, () =>
        {
            this.RuleFor(x => x.Options!.PageSize)
                .Must(BeValidPageSize)
                .WithMessage("PageSize must be A4, Letter, Legal, Tabloid, or Ledger.");

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
