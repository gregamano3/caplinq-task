using CarrierRates.Domain.Enums;

namespace CarrierRates.Application.Contracts.Carriers;

public sealed record DisableCarrierRequest(
    DisableReason Reason,
    string? ReasonDetails
);
