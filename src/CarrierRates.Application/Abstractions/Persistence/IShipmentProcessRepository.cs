namespace CarrierRates.Application.Abstractions.Persistence;

public interface IShipmentProcessRepository
{
    Task<bool> HasBlockingProcessesAsync(Guid carrierConfigId, CancellationToken cancellationToken = default);
}
