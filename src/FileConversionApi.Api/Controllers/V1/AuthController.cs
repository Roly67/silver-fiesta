// <copyright file="AuthController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Api.Models;
using FileConversionApi.Application.Commands.Auth;
using FileConversionApi.Application.DTOs;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileConversionApi.Api.Controllers.V1;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    public AuthController(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication tokens.</returns>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Invalid request or email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password);
        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Registration Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.Created(string.Empty, result.Value);
    }

    /// <summary>
    /// Logs in a user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication tokens.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Login Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Refreshes an access token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new authentication tokens.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Invalid refresh token.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await this.mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return this.BadRequest(new ProblemDetailsResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Token Refresh Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = result.Error.Message,
                ErrorCode = result.Error.Code,
            });
        }

        return this.Ok(result.Value);
    }
}
