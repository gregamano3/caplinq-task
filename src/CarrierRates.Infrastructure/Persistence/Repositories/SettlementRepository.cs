using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence.Repositories;

public class SettlementRepository(AppDbContext dbContext) : ISettlementRepository
{
    public Task<bool> HasPendingAsync(Guid carrierConfigId, CancellationToken cancellationToken = default)
    {
        return dbContext.Settlements.AnyAsync(
            x => x.CarrierConfigId == carrierConfigId && x.Status == SettlementStatus.Pending,
            cancellationToken
        );
    }
}
