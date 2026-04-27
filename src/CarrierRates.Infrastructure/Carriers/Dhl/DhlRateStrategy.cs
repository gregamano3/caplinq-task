using System.Net.Http.Json;
using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Constants;
using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers.Dhl;

public class DhlRateStrategy(
    IHttpClientFactory httpClientFactory,
    ICarrierConfigRepository carrierConfigRepository,
    DhlAdapter adapter
) : ICarrierRateStrategy
{
    public bool CanHandle(string carrierKey) => carrierKey.Equals(CarrierKeys.Dhl, StringComparison.OrdinalIgnoreCase);

    public async Task<Result<ShippingRateResponse>> GetRatesAsync(
        RateQuery query,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var config = await carrierConfigRepository.GetByCarrierKeyAsync(CarrierKeys.Dhl, cancellationToken);
            if (config is null)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.dhl.missing_config", "DHL config not found.")
                );
            }

            var payload = new DhlRateRequest(
                From: new DhlLocation(query.Origin.PostalCode, query.Origin.CountryCode),
                To: new DhlLocation(query.Destination.PostalCode, query.Destination.CountryCode),
                Parcel: new DhlParcel(
                    query.Package.Weight,
                    new DhlSize(
                        query.Package.Dimensions.Length,
                        query.Package.Dimensions.Width,
                        query.Package.Dimensions.Height
                    )
                )
            );

            var client = httpClientFactory.CreateClient("DhlClient");
            client.BaseAddress = new Uri(config.BaseUrl);
            var response = await client.PostAsJsonAsync("/api/dhl/rates", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.dhl.http_error", $"DHL API failed with {(int)response.StatusCode}.")
                );
            }

            var body = await response.Content.ReadFromJsonAsync<DhlRateResponse>(cancellationToken: cancellationToken);
            if (body is null)
            {
                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.dhl.invalid_response", "DHL API returned an empty or invalid body.")
                );
            }

            return Result<ShippingRateResponse>.Success(adapter.ToUnified(body));
        }
        catch (Exception ex)
        {
            return Result<ShippingRateResponse>.Failure(
                new Error("carrier.dhl.exception", $"DHL request failed: {ex.Message}")
            );
        }
    }
}
