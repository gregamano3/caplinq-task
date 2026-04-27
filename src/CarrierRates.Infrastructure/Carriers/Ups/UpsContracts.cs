using System.Text.Json.Serialization;

namespace CarrierRates.Infrastructure.Carriers.Ups;

public sealed record UpsRateRequest(UpsShipment Shipment);

public sealed record UpsShipment(
    string OriginPostalCode,
    string DestinationPostalCode,
    string OriginCountryCode,
    string DestinationCountryCode,
    decimal WeightLbs,
    UpsDimensions DimensionsInches
);

public sealed record UpsDimensions(decimal Length, decimal Width, decimal Height);

public sealed record UpsRateResponse(
    string Company,
    [property: JsonPropertyName("services")] IReadOnlyCollection<UpsService> Services
);

public sealed record UpsService(
    string Service,
    DateTime Eta,
    decimal Cost
);
