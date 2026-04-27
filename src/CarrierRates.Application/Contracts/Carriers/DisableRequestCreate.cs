using CarrierRates.Domain.Enums;

namespace CarrierRates.Application.Contracts.Carriers;

public sealed record DisableRequestCreate(
    DisableReason Reason,
    string? ReasonDetails
);
