using CarrierRates.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<CarrierConfig> CarrierConfigs => Set<CarrierConfig>();
    public DbSet<CarrierDisableRequest> CarrierDisableRequests => Set<CarrierDisableRequest>();
    public DbSet<ShipmentProcess> ShipmentProcesses => Set<ShipmentProcess>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
}
