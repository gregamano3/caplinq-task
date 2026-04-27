namespace CarrierRates.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "CarrierRates.Api";
    public string Audience { get; set; } = "CarrierRates.Client";
    public string SigningKey { get; set; } = "change-this-in-api-config-min-32-chars";
    public int AccessTokenMinutes { get; set; } = 60;
}
