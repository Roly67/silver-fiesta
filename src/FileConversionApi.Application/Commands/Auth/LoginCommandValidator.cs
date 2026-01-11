// <copyright file="LoginCommandValidator.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FluentValidation;

namespace FileConversionApi.Application.Commands.Auth;

/// <summary>
/// Validator for the login command.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandValidator"/> class.
    /// </summary>
    public LoginCommandValidator()
    {
        this.RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        this.RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
