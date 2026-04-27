using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Entities;
using CarrierRates.Domain.Models;
using CarrierRates.Infrastructure.Carriers;
using Moq;

namespace CarrierRates.UnitTests.Carriers;

public class CarrierRateAggregatorTests
{
    [Fact]
    public async Task QueryRatesAsync_ShouldReturnPartialSuccess_WhenOneCarrierFails()
    {
        var repo = new Mock<ICarrierConfigRepository>();
        repo.Setup(x => x.GetAllEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new CarrierConfig { CarrierKey = "fedex", IsEnabled = true },
                new CarrierConfig { CarrierKey = "ups", IsEnabled = true }
            });

        var strategies = new ICarrierRateStrategy[]
        {
            new FakeStrategy("fedex", Result<ShippingRateResponse>.Success(
                new ShippingRateResponse("FedEx", new[]
                {
                    new RateOption("Ground", DateTime.UtcNow, new Money(10, "USD"))
                })
            )),
            new FakeStrategy("ups", Result<ShippingRateResponse>.Failure(
                new Error("carrier.ups.http_error", "UPS error")
            ))
        };

        var sut = new CarrierRateAggregator(strategies, repo.Object);
        var result = await sut.QueryRatesAsync(CreateQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Single(result.CarrierErrors);
        Assert.Equal("ups", result.CarrierErrors.First().CarrierKey);
    }

    [Fact]
    public async Task QueryRatesAsync_ShouldReturnFailure_WhenAllCarriersFail()
    {
        var repo = new Mock<ICarrierConfigRepository>();
        repo.Setup(x => x.GetAllEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new CarrierConfig { CarrierKey = "fedex", IsEnabled = true }
            });

        var strategies = new ICarrierRateStrategy[]
        {
            new FakeStrategy("fedex", Result<ShippingRateResponse>.Failure(
                new Error("carrier.fedex.http_error", "FedEx failed")
            ))
        };

        var sut = new CarrierRateAggregator(strategies, repo.Object);
        var result = await sut.QueryRatesAsync(CreateQuery(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Value);
        Assert.Single(result.CarrierErrors);
    }

    private static RateQuery CreateQuery()
    {
        return new RateQuery(
            new Address("12345", "US"),
            new Address("67890", "US"),
            new PackageDetails(5, new Dimensions(10, 5, 5))
        );
    }

    private sealed class FakeStrategy(string carrierKey, Result<ShippingRateResponse> result) : ICarrierRateStrategy
    {
        public bool CanHandle(string key) => key.Equals(carrierKey, StringComparison.OrdinalIgnoreCase);

        public Task<Result<ShippingRateResponse>> GetRatesAsync(
            RateQuery query,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(result);
    }
}
