using CarrierRates.Domain.Models;

namespace CarrierRates.Application.Abstractions.Caching;

public interface IRateCache
{
    Task<IReadOnlyCollection<ShippingRateResponse>?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, IReadOnlyCollection<ShippingRateResponse> rates, TimeSpan ttl, CancellationToken cancellationToken = default);
}
