using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers.Ups;

public class UpsAdapter
{
    public ShippingRateResponse ToUnified(UpsRateResponse response)
    {
        return new ShippingRateResponse(
            Carrier: response.Company,
            RateOptions: response.Services
                .Select(
                    x => new RateOption(
                        ServiceName: x.Service,
                        EstimatedDelivery: x.Eta,
                        Price: new Money(x.Cost, "USD")
                    )
                )
                .ToArray()
        );
    }
}
