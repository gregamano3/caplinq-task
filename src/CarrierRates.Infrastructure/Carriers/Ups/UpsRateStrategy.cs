using System.Net.Http.Json;
using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Constants;
using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers.Ups;

public class UpsRateStrategy(
    IHttpClientFactory httpClientFactory,
    ICarrierConfigRepository carrierConfigRepository,
    UpsAdapter adapter
) : ICarrierRateStrategy
{
    public bool CanHandle(string carrierKey) => carrierKey.Equals(CarrierKeys.Ups, StringComparison.OrdinalIgnoreCase);

    public async Task<Result<ShippingRateResponse>> GetRatesAsync(
        RateQuery query,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var config = await carrierConfigRepository.GetByCarrierKeyAsync(CarrierKeys.Ups, cancellationToken);
            if (config is null)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.ups.missing_config", "UPS config not found.")
                );
            }

            var payload = new UpsRateRequest(
                Shipment: new UpsShipment(
                    OriginPostalCode: query.Origin.PostalCode,
                    DestinationPostalCode: query.Destination.PostalCode,
                    OriginCountryCode: query.Origin.CountryCode,
                    DestinationCountryCode: query.Destination.CountryCode,
                    WeightLbs: query.Package.Weight,
                    DimensionsInches: new UpsDimensions(
                        query.Package.Dimensions.Length,
                        query.Package.Dimensions.Width,
                        query.Package.Dimensions.Height
                    )
                )
            );

            var client = httpClientFactory.CreateClient("UpsClient");
            client.BaseAddress = new Uri(config.BaseUrl);
            var response = await client.PostAsJsonAsync("/api/ups/shipping-rates", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.ups.http_error", $"UPS API failed with {(int)response.StatusCode}.")
                );
            }

            var body = await response.Content.ReadFromJsonAsync<UpsRateResponse>(cancellationToken: cancellationToken);
            if (body is null)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.ups.invalid_response", "UPS API returned an empty or invalid body.")
                );
            }

            return Result<ShippingRateResponse>.Success(adapter.ToUnified(body));
        }
        catch (Exception ex)
        {
            return Result<ShippingRateResponse>.Failure(
                new Error("carrier.ups.exception", $"UPS request failed: {ex.Message}")
            );
        }
    }
}
