using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarrierRates.Infrastructure.Persistence.Repositories;

public class ShipmentProcessRepository(AppDbContext dbContext) : IShipmentProcessRepository
{
    public Task<bool> HasBlockingProcessesAsync(Guid carrierConfigId, CancellationToken cancellationToken = default)
    {
        return dbContext.ShipmentProcesses.AnyAsync(
            x => x.CarrierConfigId == carrierConfigId && x.Status == ShipmentProcessStatus.AwaitingConfirmation,
            cancellationToken
        );
    }
}
