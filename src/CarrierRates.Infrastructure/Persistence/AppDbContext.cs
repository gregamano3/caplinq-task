using CarrierRates.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// User store used for login and role-based authorization.
    /// </summary>
    public DbSet<AppUser> Users => Set<AppUser>();
    /// <summary>
    /// Carrier configuration records (base URL, API key, enabled state).
    /// </summary>
    public DbSet<CarrierConfig> CarrierConfigs => Set<CarrierConfig>();
    /// <summary>
    /// Disable request audit trail.
    /// </summary>
    public DbSet<CarrierDisableRequest> CarrierDisableRequests => Set<CarrierDisableRequest>();
    /// <summary>
    /// Shipment process records used to evaluate disable blocking rules.
    /// </summary>
    public DbSet<ShipmentProcess> ShipmentProcesses => Set<ShipmentProcess>();
    /// <summary>
    /// Settlement records used to evaluate disable blocking rules.
    /// </summary>
    public DbSet<Settlement> Settlements => Set<Settlement>();
}
