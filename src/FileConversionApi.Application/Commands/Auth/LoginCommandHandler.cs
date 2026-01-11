// <copyright file="LoginCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Auth;

/// <summary>
/// Handles the login command.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;
    private readonly IJwtTokenGenerator jwtTokenGenerator;
    private readonly ILogger<LoginCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="jwtTokenGenerator">The JWT token generator.</param>
    /// <param name="logger">The logger.</param>
    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginCommandHandler> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        this.jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<TokenResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Login attempt for email {Email}", request.Email);

        var user = await this.userRepository
            .GetByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            this.logger.LogWarning("Login failed - user not found for email {Email}", request.Email);
            return UserErrors.InvalidCredentials;
        }

        if (!user.IsActive)
        {
            this.logger.LogWarning("Login failed - user {UserId} is inactive", user.Id);
            return UserErrors.Inactive;
        }

        if (!this.passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            this.logger.LogWarning("Login failed - invalid password for user {UserId}", user.Id);
            return UserErrors.InvalidCredentials;
        }

        this.logger.LogInformation("User {UserId} logged in successfully", user.Id);

        var tokenResponse = this.jwtTokenGenerator.GenerateToken(user);
        return tokenResponse;
    }
}
