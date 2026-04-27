namespace CarrierRates.Domain.Models;

public sealed record ShippingRateResponse(
    string Carrier,
    IReadOnlyCollection<RateOption> RateOptions
);

public sealed record RateOption(
    string ServiceName,
    DateTime EstimatedDelivery,
    Money Price
);

public sealed record Money(
    decimal Amount,
    string Currency
);
