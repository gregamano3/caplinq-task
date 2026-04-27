using CarrierRates.Application.Abstractions.Auth;
using CarrierRates.Application.Abstractions.Caching;
using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Abstractions.Services;
using CarrierRates.Infrastructure.Auth;
using CarrierRates.Infrastructure.Caching;
using CarrierRates.Infrastructure.Carriers;
using CarrierRates.Infrastructure.Carriers.Dhl;
using CarrierRates.Infrastructure.Carriers.FedEx;
using CarrierRates.Infrastructure.Carriers.Ups;
using CarrierRates.Infrastructure.Persistence;
using CarrierRates.Infrastructure.Persistence.Repositories;
using CarrierRates.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarrierRates.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services, persistence, integrations, and concrete use-case implementations.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("CarrierRatesDb"));
        services.AddMemoryCache();
        services.AddHttpClient();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<ICarrierConfigRepository, CarrierConfigRepository>();
        services.AddScoped<ICarrierDisableRequestRepository, CarrierDisableRequestRepository>();
        services.AddScoped<IShipmentProcessRepository, ShipmentProcessRepository>();
        services.AddScoped<ISettlementRepository, SettlementRepository>();

        services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRateCache, MemoryRateCache>();

        services.AddScoped<FedExAdapter>();
        services.AddScoped<DhlAdapter>();
        services.AddScoped<UpsAdapter>();

        services.AddScoped<ICarrierRateStrategy, FedExRateStrategy>();
        services.AddScoped<ICarrierRateStrategy, DhlRateStrategy>();
        services.AddScoped<ICarrierRateStrategy, UpsRateStrategy>();
        services.AddScoped<ICarrierRateAggregator, CarrierRateAggregator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICarrierManagementService, CarrierManagementService>();
        services.AddScoped<IRateQueryService, RateQueryService>();

        services.AddHttpClient("FedExClient");
        services.AddHttpClient("DhlClient");
        services.AddHttpClient("UpsClient");

        return services;
    }
}
