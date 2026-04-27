using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers.Dhl;

public class DhlAdapter
{
    /// <summary>
    /// Maps DHL-specific rate payloads into the unified domain response model.
    /// </summary>
    public ShippingRateResponse ToUnified(DhlRateResponse response)
    {
        return new ShippingRateResponse(
            Carrier: response.Provider,
            RateOptions: response.Options
                .Select(
                    x => new RateOption(
                        ServiceName: x.Name,
                        EstimatedDelivery: x.DeliveryDate,
                        Price: new Money(x.Price, "USD")
                    )
                )
                .ToArray()
        );
    }
}
