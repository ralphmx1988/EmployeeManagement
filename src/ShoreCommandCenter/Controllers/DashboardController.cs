using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShoreCommandCenter.Models;
using ShoreCommandCenter.Services;

namespace ShoreCommandCenter.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IFleetService _fleetService;
    private readonly IHealthMetricsService _healthMetricsService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IFleetService fleetService,
        IHealthMetricsService healthMetricsService,
        ILogger<DashboardController> logger)
    {
        _fleetService = fleetService;
        _healthMetricsService = healthMetricsService;
        _logger = logger;
    }

    [HttpGet("fleet-status")]
    public async Task<IActionResult> GetFleetStatus()
    {
        try
        {
            var status = await _fleetService.GetFleetStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get fleet status");
            return StatusCode(500, new { error = "Failed to retrieve fleet status" });
        }
    }

    [HttpGet("ships/{shipId}/health/current")]
    public async Task<IActionResult> GetCurrentShipHealth(string shipId)
    {
        try
        {
            var health = await _healthMetricsService.GetCurrentHealthAsync(shipId);
            if (health == null)
                return NotFound(new { error = "Ship health data not found" });

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current health for ship {ShipId}", shipId);
            return StatusCode(500, new { error = "Failed to retrieve ship health" });
        }
    }

    [HttpGet("ships/{shipId}/health/history")]
    public async Task<IActionResult> GetShipHealthHistory(string shipId, [FromQuery] int days = 7)
    {
        try
        {
            var history = await _healthMetricsService.GetHealthHistoryAsync(shipId, days);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get health history for ship {ShipId}", shipId);
            return StatusCode(500, new { error = "Failed to retrieve ship health history" });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
