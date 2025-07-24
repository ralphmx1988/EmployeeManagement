using Microsoft.EntityFrameworkCore;
using ShoreCommandCenter.Data;
using ShoreCommandCenter.Models;
using System.Text.Json;

namespace ShoreCommandCenter.Services;

public class ShipService : IShipService
{
    private readonly FleetDbContext _context;
    private readonly ILogger<ShipService> _logger;

    public ShipService(FleetDbContext context, ILogger<ShipService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Ship> RegisterShipAsync(ShipRegistrationRequest request)
    {
        var existingShip = await _context.Ships.FindAsync(request.ShipId);
        
        if (existingShip != null)
        {
            // Update existing ship
            existingShip.Name = request.ShipName;
            existingShip.LastSeen = request.LastSeen;
            existingShip.Status = "Online";
            existingShip.UpdatedAt = DateTime.UtcNow;
            existingShip.Latitude = request.Latitude;
            existingShip.Longitude = request.Longitude;
            existingShip.TimeZone = request.TimeZone;
            
            _logger.LogInformation("Updated existing ship registration: {ShipId}", request.ShipId);
        }
        else
        {
            // Create new ship
            existingShip = new Ship
            {
                Id = request.ShipId,
                Name = request.ShipName,
                Status = "Online",
                LastSeen = request.LastSeen,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                TimeZone = request.TimeZone,
                ConfigurationJson = JsonSerializer.Serialize(new
                {
                    agentVersion = request.AgentVersion,
                    capabilities = request.Capabilities,
                    timezone = request.TimeZone
                })
            };
            
            _context.Ships.Add(existingShip);
            _logger.LogInformation("Created new ship registration: {ShipId}", request.ShipId);
        }

        await _context.SaveChangesAsync();
        return existingShip;
    }

    public async Task<Ship?> GetShipAsync(string shipId)
    {
        return await _context.Ships.FindAsync(shipId);
    }

    public async Task<IEnumerable<Ship>> GetAllShipsAsync()
    {
        return await _context.Ships
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ship>> GetOutdatedShipsAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-15); // Ships offline for more than 15 minutes
        
        return await _context.Ships
            .Where(s => s.LastSeen < cutoffTime || s.Status == "Offline")
            .OrderBy(s => s.LastSeen)
            .ToListAsync();
    }

    public async Task UpdateLastSeenAsync(string shipId)
    {
        var ship = await _context.Ships.FindAsync(shipId);
        if (ship != null)
        {
            ship.LastSeen = DateTime.UtcNow;
            ship.Status = "Online";
            ship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateShipStatusAsync(string shipId, string status)
    {
        var ship = await _context.Ships.FindAsync(shipId);
        if (ship != null)
        {
            ship.Status = status;
            ship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateShipVersionAsync(string shipId, string? currentVersion, string? targetVersion)
    {
        var ship = await _context.Ships.FindAsync(shipId);
        if (ship != null)
        {
            ship.CurrentVersion = currentVersion;
            ship.TargetVersion = targetVersion;
            ship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
