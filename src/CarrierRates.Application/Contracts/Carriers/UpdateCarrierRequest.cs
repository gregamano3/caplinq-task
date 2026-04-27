namespace CarrierRates.Application.Contracts.Carriers;

public sealed record UpdateCarrierRequest(
    string DisplayName,
    string BaseUrl,
    string ApiKey,
    bool IsEnabled
);
