namespace CarrierRates.Application.Contracts.Carriers;

public sealed record CreateCarrierRequest(
    string CarrierKey,
    string DisplayName,
    string BaseUrl,
    string ApiKey,
    bool IsEnabled
);
