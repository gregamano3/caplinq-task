using System.Text.Json.Serialization;

namespace CarrierRates.Infrastructure.Carriers.FedEx;

public sealed record FedExRateRequest(
    FedExAddress Origin,
    FedExAddress Destination,
    FedExPackage Package
);

public sealed record FedExAddress(string PostalCode, string CountryCode);

public sealed record FedExPackage(decimal Weight, FedExDimensions Dimensions);

public sealed record FedExDimensions(decimal Length, decimal Width, decimal Height);

public sealed record FedExRateResponse(
    string Carrier,
    [property: JsonPropertyName("serviceOptions")] IReadOnlyCollection<FedExServiceOption> ServiceOptions
);

public sealed record FedExServiceOption(
    string ServiceName,
    DateTime EstimatedDelivery,
    decimal Rate
);
