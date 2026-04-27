using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CarrierRates.Application.Abstractions.Identity;
using CarrierRates.Domain.Enums;

namespace CarrierRates.Api.Security;

public class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public string UserId => GetClaim(ClaimTypes.NameIdentifier)
        ?? GetClaim(JwtRegisteredClaimNames.Sub)
        ?? GetClaim(ClaimTypes.Name)
        ?? string.Empty;
    public string Email => GetClaim(ClaimTypes.Email) ?? string.Empty;
    public UserRole Role => string.Equals(GetClaim(ClaimTypes.Role), "Admin", StringComparison.OrdinalIgnoreCase)
        ? UserRole.Admin
        : UserRole.User;

    private string? GetClaim(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }
}
