using CarrierRates.Application.Abstractions.Caching;
using CarrierRates.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CarrierRates.Infrastructure.Caching;

public class MemoryRateCache(IMemoryCache memoryCache) : IRateCache
{
    /// <summary>
    /// Gets cached rate results by deterministic query key.
    /// </summary>
    public Task<IReadOnlyCollection<ShippingRateResponse>?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        memoryCache.TryGetValue(key, out IReadOnlyCollection<ShippingRateResponse>? result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Stores rate results with a TTL to reduce repeated outbound API calls.
    /// </summary>
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
