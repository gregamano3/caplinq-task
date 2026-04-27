namespace CarrierRates.Domain.Models;

public sealed record RateQuery(
    Address Origin,
    Address Destination,
    PackageDetails Package
);

public sealed record Address(
    string PostalCode,
    string CountryCode
);

public sealed record PackageDetails(
    decimal Weight,
    Dimensions Dimensions
);

public sealed record Dimensions(
    decimal Length,
    decimal Width,
    decimal Height
);
