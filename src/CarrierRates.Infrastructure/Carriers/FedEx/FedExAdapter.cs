using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers.FedEx;

public class FedExAdapter
{
    public ShippingRateResponse ToUnified(FedExRateResponse response)
    {
        return new ShippingRateResponse(
            Carrier: response.Carrier,
            RateOptions: response.ServiceOptions
                .Select(
                    x => new RateOption(
                        ServiceName: x.ServiceName,
                        EstimatedDelivery: x.EstimatedDelivery,
                        Price: new Money(x.Rate, "USD")
                    )
                )
                .ToArray()
        );
    }
}
