using CarrierRates.Domain.Entities;

namespace CarrierRates.Application.Abstractions.Persistence;

public interface ICarrierConfigRepository
{
    Task<IReadOnlyCollection<CarrierConfig>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CarrierConfig>> GetAllEnabledAsync(CancellationToken cancellationToken = default);
    Task<CarrierConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CarrierConfig?> GetByCarrierKeyAsync(string carrierKey, CancellationToken cancellationToken = default);
    Task AddAsync(CarrierConfig entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(CarrierConfig entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountEnabledAsync(CancellationToken cancellationToken = default);
}
