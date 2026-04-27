using System.Text;
using CarrierRates.Application.Abstractions.Caching;
using CarrierRates.Application.Abstractions.Services;
using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Services;

public class RateQueryService(ICarrierRateAggregator aggregator, IRateCache rateCache) : IRateQueryService
{
    public async Task<AggregateResult<IReadOnlyCollection<ShippingRateResponse>>> QueryAsync(
        RateQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var cacheKey = BuildCacheKey(request);
        var cached = await rateCache.GetAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return new AggregateResult<IReadOnlyCollection<ShippingRateResponse>>
            {
                IsSuccess = true,
                Value = cached
            };
        }

        var aggregateResult = await aggregator.QueryRatesAsync(request, cancellationToken);
        if (aggregateResult.IsSuccess)
        {
            await rateCache.SetAsync(cacheKey, aggregateResult.Value, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return aggregateResult;
    }

    private static string BuildCacheKey(RateQuery request)
    {
        var sb = new StringBuilder("rates:");
        sb.Append(request.Origin.CountryCode).Append(':').Append(request.Origin.PostalCode).Append('|');
        sb.Append(request.Destination.CountryCode).Append(':').Append(request.Destination.PostalCode).Append('|');
        sb.Append(request.Package.Weight).Append('|');
        sb.Append(request.Package.Dimensions.Length).Append('x')
            .Append(request.Package.Dimensions.Width).Append('x')
            .Append(request.Package.Dimensions.Height);
        return sb.ToString().ToLowerInvariant();
    }
}
