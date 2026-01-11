// <copyright file="ICurrentUserService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Interface for accessing the current user context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    UserId? UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
