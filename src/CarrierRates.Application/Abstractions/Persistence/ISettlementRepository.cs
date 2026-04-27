namespace CarrierRates.Application.Abstractions.Persistence;

public interface ISettlementRepository
{
    Task<bool> HasPendingAsync(Guid carrierConfigId, CancellationToken cancellationToken = default);
}
