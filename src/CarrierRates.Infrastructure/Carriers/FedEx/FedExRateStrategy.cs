using System.Net.Http.Json;
using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Constants;
using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers.FedEx;

public class FedExRateStrategy(
    IHttpClientFactory httpClientFactory,
    ICarrierConfigRepository carrierConfigRepository,
    FedExAdapter adapter
) : ICarrierRateStrategy
{
    public bool CanHandle(string carrierKey) => carrierKey.Equals(CarrierKeys.FedEx, StringComparison.OrdinalIgnoreCase);

    public async Task<Result<ShippingRateResponse>> GetRatesAsync(
        RateQuery query,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var config = await carrierConfigRepository.GetByCarrierKeyAsync(CarrierKeys.FedEx, cancellationToken);
            if (config is null)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.fedex.missing_config", "FedEx config not found.")
                );
            }

            var payload = new FedExRateRequest(
                Origin: new FedExAddress(query.Origin.PostalCode, query.Origin.CountryCode),
                Destination: new FedExAddress(query.Destination.PostalCode, query.Destination.CountryCode),
                Package: new FedExPackage(
                    query.Package.Weight,
                    new FedExDimensions(
                        query.Package.Dimensions.Length,
                        query.Package.Dimensions.Width,
                        query.Package.Dimensions.Height
                    )
                )
            );

            var client = httpClientFactory.CreateClient("FedExClient");
            client.BaseAddress = new Uri(config.BaseUrl);
            var response = await client.PostAsJsonAsync("/api/fedex/rates", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.fedex.http_error", $"FedEx API failed with {(int)response.StatusCode}.")
                );
            }

            var body = await response.Content.ReadFromJsonAsync<FedExRateResponse>(cancellationToken: cancellationToken);
            if (body is null)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.fedex.invalid_response", "FedEx API returned an empty or invalid body.")
                );
            }

            return Result<ShippingRateResponse>.Success(adapter.ToUnified(body));
        }
        catch (Exception ex)
        {
            return Result<ShippingRateResponse>.Failure(
                new Error("carrier.fedex.exception", $"FedEx request failed: {ex.Message}")
            );
        }
    }
}
