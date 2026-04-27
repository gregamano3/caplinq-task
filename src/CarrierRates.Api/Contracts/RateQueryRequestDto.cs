namespace CarrierRates.Api.Contracts;

public sealed record RateQueryRequestDto(
    AddressDto Origin,
    AddressDto Destination,
    PackageDetailsDto Package
);

public sealed record AddressDto(string PostalCode, string CountryCode);

public sealed record PackageDetailsDto(decimal Weight, DimensionsDto Dimensions);

public sealed record DimensionsDto(decimal Length, decimal Width, decimal Height);
