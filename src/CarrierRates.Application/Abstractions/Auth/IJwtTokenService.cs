using CarrierRates.Domain.Entities;

namespace CarrierRates.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string GenerateToken(AppUser user, DateTime expiresUtc);
}
