namespace CarrierRates.Api.Contracts;

public record ShippingRateResponseDto
{
    public string Carrier { get; init; } = string.Empty;
    public IEnumerable<RateOptionDto> RateOptions { get; init; } = [];
}

public record RateOptionDto
{
    public string ServiceName { get; init; } = string.Empty;
    public DateTime EstimatedDelivery { get; init; }
    public MoneyDto Price { get; init; } = new(0, "USD");
}

public record MoneyDto(decimal Amount, string Currency);
