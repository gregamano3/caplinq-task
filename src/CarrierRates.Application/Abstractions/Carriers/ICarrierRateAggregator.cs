using CarrierRates.Application.Common;
using CarrierRates.Domain.Models;

namespace CarrierRates.Application.Abstractions.Carriers;

public interface ICarrierRateAggregator
{
    Task<AggregateResult<IReadOnlyCollection<ShippingRateResponse>>> QueryRatesAsync(
        RateQuery query,
        CancellationToken cancellationToken = default
    );
}
