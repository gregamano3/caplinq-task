using CarrierRates.Application.Abstractions.Persistence;

namespace CarrierRates.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    /// <summary>
    /// Commits pending persistence changes as one unit.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
