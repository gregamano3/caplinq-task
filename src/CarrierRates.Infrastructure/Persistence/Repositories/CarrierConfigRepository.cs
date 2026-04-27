using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence.Repositories;

public class CarrierConfigRepository(AppDbContext dbContext) : ICarrierConfigRepository
{
    /// <summary>
    /// Returns all carrier configurations.
    /// </summary>
    public async Task<IReadOnlyCollection<CarrierConfig>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CarrierConfigs
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns enabled carrier configurations only.
    /// </summary>
    public async Task<IReadOnlyCollection<CarrierConfig>> GetAllEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CarrierConfigs
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public Task<CarrierConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CarrierConfigs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<CarrierConfig?> GetByCarrierKeyAsync(string carrierKey, CancellationToken cancellationToken = default)
    {
        var normalized = carrierKey.Trim().ToLower();
        return dbContext.CarrierConfigs.FirstOrDefaultAsync(x => x.CarrierKey == normalized, cancellationToken);
    }

    public async Task AddAsync(CarrierConfig entity, CancellationToken cancellationToken = default)
    {
        await dbContext.CarrierConfigs.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(CarrierConfig entity, CancellationToken cancellationToken = default)
    {
        dbContext.CarrierConfigs.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var carrier = await GetByIdAsync(id, cancellationToken);
        if (carrier is null)
        {
            return;
        }

        dbContext.CarrierConfigs.Remove(carrier);
    }

    public Task<int> CountEnabledAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.CarrierConfigs.CountAsync(x => x.IsEnabled, cancellationToken);
    }
}
