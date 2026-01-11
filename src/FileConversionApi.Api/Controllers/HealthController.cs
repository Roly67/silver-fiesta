// <copyright file="HealthController.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileConversionApi.Api.Controllers;

/// <summary>
/// Controller for health check endpoints.
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Gets the health status of the API.
    /// </summary>
    /// <returns>Health status.</returns>
    /// <response code="200">API is healthy.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return this.Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTimeOffset.UtcNow,
        });
    }
}
