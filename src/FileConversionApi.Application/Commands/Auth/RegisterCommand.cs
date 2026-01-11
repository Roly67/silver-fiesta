// <copyright file="RegisterCommand.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.DTOs;
using FileConversionApi.Domain.Primitives;
using MediatR;

namespace FileConversionApi.Application.Commands.Auth;

/// <summary>
/// Command to register a new user.
/// </summary>
/// <param name="Email">The email address.</param>
/// <param name="Password">The password.</param>
public record RegisterCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;
