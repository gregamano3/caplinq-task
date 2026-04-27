namespace CarrierRates.Application.Contracts.Auth;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresUtc);
