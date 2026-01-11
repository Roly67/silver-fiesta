// <copyright file="RefreshTokenCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;
using MediatR;

namespace FileConversionApi.Application.Commands.Auth;

/// <summary>
/// Command to refresh an access token.
/// </summary>
/// <param name="RefreshToken">The refresh token.</param>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<TokenResponse>>;
