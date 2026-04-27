using System.Net;
using System.Net.Http.Json;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Domain.Constants;
using CarrierRates.Domain.Entities;
using CarrierRates.Domain.Models;
using CarrierRates.Infrastructure.Carriers.FedEx;
using Moq;

namespace CarrierRates.UnitTests.Carriers;

public class FedExRateStrategyRetryTests
{
    [Fact]
    public async Task GetRatesAsync_ShouldRetryAndSucceed_AfterTransientFailure()
    {
        var repo = new Mock<ICarrierConfigRepository>();
        repo.Setup(x => x.GetByCarrierKeyAsync(CarrierKeys.FedEx, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CarrierConfig
            {
                CarrierKey = CarrierKeys.FedEx,
                BaseUrl = "http://localhost"
            });

        var handler = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new FedExRateResponse(
                    "FedEx",
                    new[]
                    {
                        new FedExServiceOption("FedEx Ground", DateTime.UtcNow.Date.AddDays(2), 12.34m)
                    }
                ))
            }
        );

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient("FedExClient")).Returns(client);

        var sut = new FedExRateStrategy(factory.Object, repo.Object, new FedExAdapter());
        var result = await sut.GetRatesAsync(CreateQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, handler.CallCount);
    }

    private static RateQuery CreateQuery()
    {
        return new RateQuery(
            new Address("12345", "US"),
            new Address("67890", "US"),
            new PackageDetails(5, new Dimensions(10, 5, 5))
        );
    }

    private sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (_responses.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return Task.FromResult(_responses.Dequeue());
        }
    }
}
