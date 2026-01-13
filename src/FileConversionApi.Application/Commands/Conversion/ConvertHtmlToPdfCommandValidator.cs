// <copyright file="ConvertHtmlToPdfCommandValidator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

using FluentValidation;

namespace FileConversionApi.Application.Commands.Conversion;

/// <summary>
/// Validator for the convert HTML to PDF command.
/// </summary>
public class ConvertHtmlToPdfCommandValidator : AbstractValidator<ConvertHtmlToPdfCommand>
{
    private readonly IInputValidationService? validationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToPdfCommandValidator"/> class.
    /// </summary>
    public ConvertHtmlToPdfCommandValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertHtmlToPdfCommandValidator"/> class.
    /// </summary>
    /// <param name="validationService">The input validation service.</param>
    public ConvertHtmlToPdfCommandValidator(IInputValidationService? validationService)
    {
        this.validationService = validationService;

        this.RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.HtmlContent) || !string.IsNullOrWhiteSpace(x.Url))
            .WithMessage("Either HtmlContent or Url must be provided.");

        this.When(x => !string.IsNullOrWhiteSpace(x.Url), () =>
        {
            this.RuleFor(x => x.Url)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Url must be a valid HTTP or HTTPS URL.");

            this.RuleFor(x => x.Url)
                .Must(this.BeAllowedUrl)
                .WithMessage(x => this.GetUrlValidationErrorMessage(x.Url));
        });

        this.When(x => !string.IsNullOrWhiteSpace(x.HtmlContent), () =>
        {
            this.RuleFor(x => x.HtmlContent)
                .Must(this.BeWithinHtmlSizeLimit)
                .WithMessage(x => $"HtmlContent must not exceed {this.GetMaxHtmlSizeMb():F0}MB.");
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

    private bool BeAllowedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || this.validationService is null)
        {
            return true;
        }

        var result = this.validationService.ValidateUrl(url);
        return result.IsSuccess;
    }

    private string GetUrlValidationErrorMessage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || this.validationService is null)
        {
            return "Invalid URL.";
        }

        var result = this.validationService.ValidateUrl(url);
        return result.IsFailure ? result.Error.Message : "Invalid URL.";
    }

    private bool BeWithinHtmlSizeLimit(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return true;
        }

        if (this.validationService is null)
        {
            // Default limit of 10MB
            return content.Length <= 10 * 1024 * 1024;
        }

        var result = this.validationService.ValidateHtmlContentSize(content);
        return result.IsSuccess;
    }

    private double GetMaxHtmlSizeMb()
    {
        if (this.validationService is null)
        {
            return 10;
        }

        return this.validationService.GetMaxHtmlContentBytes() / (1024.0 * 1024.0);
    }
}
