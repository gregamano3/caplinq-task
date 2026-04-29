using CarrierRates.Application.Abstractions.Auth;
using CarrierRates.Domain.Constants;
using CarrierRates.Domain.Entities;
using CarrierRates.Domain.Enums;

namespace CarrierRates.Infrastructure.Persistence;

public static class AppDbSeeder
{
    /// <summary>
    /// Seeds default carriers and users when running with an empty in-memory database.
    /// </summary>
    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        string? carrierBaseUrl = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedCarrierBaseUrl = string.IsNullOrWhiteSpace(carrierBaseUrl)
            ? "https://localhost:5001"
            : carrierBaseUrl.TrimEnd('/');

        if (!dbContext.CarrierConfigs.Any())
        {
            dbContext.CarrierConfigs.AddRange(
                new CarrierConfig
                {
                    CarrierKey = CarrierKeys.FedEx,
                    DisplayName = "FedEx",
                    BaseUrl = resolvedCarrierBaseUrl,
                    ApiKey = "fedex-mock-key",
                    IsEnabled = true
                },
                new CarrierConfig
                {
                    CarrierKey = CarrierKeys.Ups,
                    DisplayName = "UPS",
                    BaseUrl = resolvedCarrierBaseUrl,
                    ApiKey = "ups-mock-key",
                    IsEnabled = true
                },
                new CarrierConfig
                {
                    CarrierKey = CarrierKeys.Dhl,
                    DisplayName = "DHL",
                    BaseUrl = resolvedCarrierBaseUrl,
                    ApiKey = "dhl-mock-key",
                    IsEnabled = true
                }
            );
        }

        if (!dbContext.Users.Any())
        {
            dbContext.Users.AddRange(
                new AppUser
                {
                    Email = "admin@example.com",
                    PasswordHash = passwordHasher.Hash("password1234"),
                    Role = UserRole.Admin,
                    IsActive = true
                },
                new AppUser
                {
                    Email = "user@test.com",
                    PasswordHash = passwordHasher.Hash("User123!"),
                    Role = UserRole.User,
                    IsActive = true
                }
            );
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
