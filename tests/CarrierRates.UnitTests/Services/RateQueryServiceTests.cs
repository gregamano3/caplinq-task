using CarrierRates.Application.Abstractions.Caching;
using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Models;
using CarrierRates.Infrastructure.Services;
using Moq;

namespace CarrierRates.UnitTests.Services;

public class RateQueryServiceTests
{
    [Fact]
    public async Task QueryAsync_ShouldReturnCachedResult_WhenCacheHit()
    {
        var aggregator = new Mock<ICarrierRateAggregator>(MockBehavior.Strict);
        var cache = new Mock<IRateCache>();
        var query = CreateQuery();
        var cached = new[]
        {
            new ShippingRateResponse("FedEx", new[]
            {
                new RateOption("Ground", DateTime.UtcNow.AddDays(2), new Money(10, "USD"))
            })
        };

        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var sut = new RateQueryService(aggregator.Object, cache.Object);
        var result = await sut.QueryAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        aggregator.Verify(x => x.QueryRatesAsync(It.IsAny<RateQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task QueryAsync_ShouldCacheResult_WhenAggregatorSucceeds()
    {
        var aggregator = new Mock<ICarrierRateAggregator>();
        var cache = new Mock<IRateCache>();
        var query = CreateQuery();

        cache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ShippingRateResponse>?)null);

        aggregator.Setup(x => x.QueryRatesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AggregateResult<IReadOnlyCollection<ShippingRateResponse>>
            {
                IsSuccess = true,
                Value = new[]
                {
                    new ShippingRateResponse("UPS", new[]
                    {
                        new RateOption("2nd Day", DateTime.UtcNow.AddDays(3), new Money(20, "USD"))
                    })
                }
            });

        var sut = new RateQueryService(aggregator.Object, cache.Object);
        var result = await sut.QueryAsync(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        cache.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.Is<IReadOnlyCollection<ShippingRateResponse>>(r => r.Count == 1),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    private static RateQuery CreateQuery()
    {
        return new RateQuery(
            new Address("12345", "US"),
            new Address("67890", "US"),
            new PackageDetails(5, new Dimensions(10, 5, 5))
        );
    }
}
