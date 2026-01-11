// <copyright file="CurrentUserService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Security.Claims;

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Api.Services;

/// <summary>
/// Service for accessing the current user context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    public UserId? UserId
    {
        get
        {
            var userIdClaim = this.httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return Domain.ValueObjects.UserId.From(userId);
        }
    }

    /// <inheritdoc/>
    public bool IsAuthenticated =>
        this.httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
