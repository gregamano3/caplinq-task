using CarrierRates.Application.Common;
using CarrierRates.Domain.Models;

namespace CarrierRates.Application.Abstractions.Carriers;

public interface ICarrierRateStrategy
{
    bool CanHandle(string carrierKey);
    Task<Result<ShippingRateResponse>> GetRatesAsync(
        RateQuery query,
        CancellationToken cancellationToken = default
    );
}
