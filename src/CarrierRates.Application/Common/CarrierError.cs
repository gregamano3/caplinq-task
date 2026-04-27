namespace CarrierRates.Application.Common;

public sealed record CarrierError(string CarrierKey, string Code, string Message);
