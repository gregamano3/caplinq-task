using CarrierRates.Domain.Entities;

namespace CarrierRates.Application.Abstractions.Persistence;

public interface ICarrierDisableRequestRepository
{
    Task AddAsync(CarrierDisableRequest entity, CancellationToken cancellationToken = default);
    Task<CarrierDisableRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(CarrierDisableRequest entity, CancellationToken cancellationToken = default);
}
