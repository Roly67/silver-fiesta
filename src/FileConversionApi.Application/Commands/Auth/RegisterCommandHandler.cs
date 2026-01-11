// <copyright file="RegisterCommandHandler.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Domain.Errors;
using FileConversionApi.Domain.Primitives;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FileConversionApi.Application.Commands.Auth;

/// <summary>
/// Handles the register command.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<TokenResponse>>
{
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IPasswordHasher passwordHasher;
    private readonly IJwtTokenGenerator jwtTokenGenerator;
    private readonly ILogger<RegisterCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="passwordHasher">The password hasher.</param>
    /// <param name="jwtTokenGenerator">The JWT token generator.</param>
    /// <param name="logger">The logger.</param>
    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RegisterCommandHandler> logger)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        this.jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Result<TokenResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Registering new user with email {Email}", request.Email);

        var emailExists = await this.userRepository
            .EmailExistsAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (emailExists)
        {
            this.logger.LogWarning("Registration failed - email {Email} already exists", request.Email);
            return UserErrors.EmailAlreadyExists;
        }

        var passwordHash = this.passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash);

        await this.userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await this.unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        this.logger.LogInformation("User {UserId} registered successfully", user.Id);

        var tokenResponse = this.jwtTokenGenerator.GenerateToken(user);
        return tokenResponse;
    }
}
