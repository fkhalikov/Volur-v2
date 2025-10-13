using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Volur.Api.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Health check endpoint (liveness).
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness check endpoint.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();

        if (healthReport.Status == HealthStatus.Healthy)
        {
            return Ok(new
            {
                status = "Healthy",
                checks = healthReport.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                timestamp = DateTime.UtcNow
            });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            status = healthReport.Status.ToString(),
            checks = healthReport.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            timestamp = DateTime.UtcNow
        });
    }
}

