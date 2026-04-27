using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence.Repositories;

public class CarrierDisableRequestRepository(AppDbContext dbContext) : ICarrierDisableRequestRepository
{
    /// <summary>
    /// Adds a new carrier disable request.
    /// </summary>
    public async Task AddAsync(CarrierDisableRequest entity, CancellationToken cancellationToken = default)
    {
        await dbContext.CarrierDisableRequests.AddAsync(entity, cancellationToken);
    }

    public Task<CarrierDisableRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.CarrierDisableRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task UpdateAsync(CarrierDisableRequest entity, CancellationToken cancellationToken = default)
    {
        dbContext.CarrierDisableRequests.Update(entity);
        return Task.CompletedTask;
    }
}
