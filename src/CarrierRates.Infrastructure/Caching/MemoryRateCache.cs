using CarrierRates.Application.Abstractions.Caching;
using CarrierRates.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CarrierRates.Infrastructure.Caching;

public class MemoryRateCache(IMemoryCache memoryCache) : IRateCache
{
    public Task<IReadOnlyCollection<ShippingRateResponse>?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        memoryCache.TryGetValue(key, out IReadOnlyCollection<ShippingRateResponse>? result);
        return Task.FromResult(result);
    }

    public Task SetAsync(
        string key,
        IReadOnlyCollection<ShippingRateResponse> rates,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        memoryCache.Set(key, rates, ttl);
        return Task.CompletedTask;
    }
}
