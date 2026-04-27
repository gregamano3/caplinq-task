using CarrierRates.Application.Common;
using CarrierRates.Domain.Models;

namespace CarrierRates.Application.Abstractions.Services;

public interface IRateQueryService
{
    Task<AggregateResult<IReadOnlyCollection<ShippingRateResponse>>> QueryAsync(
        RateQuery request,
        CancellationToken cancellationToken = default
    );
}
