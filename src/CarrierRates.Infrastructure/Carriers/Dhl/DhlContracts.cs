using System.Text.Json.Serialization;

namespace CarrierRates.Infrastructure.Carriers.Dhl;

/// <summary>
/// DHL-specific request contract for rate retrieval.
/// </summary>
public sealed record DhlRateRequest(DhlLocation From, DhlLocation To, DhlParcel Parcel);

public sealed record DhlLocation(string ZipCode, string Country);

public sealed record DhlParcel(decimal WeightKg, DhlSize SizeCm);

public sealed record DhlSize(decimal Length, decimal Width, decimal Height);

/// <summary>
/// DHL-specific response contract for rate retrieval.
/// </summary>
public sealed record DhlRateResponse(
    string Provider,
    [property: JsonPropertyName("options")] IReadOnlyCollection<DhlOption> Options
);

public sealed record DhlOption(
    string Name,
    DateTime DeliveryDate,
    decimal Price
);
