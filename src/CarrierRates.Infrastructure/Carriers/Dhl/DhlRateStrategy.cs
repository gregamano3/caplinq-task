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
    private const int MaxAttempts = 3;

    public bool CanHandle(string carrierKey) => carrierKey.Equals(CarrierKeys.Dhl, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Fetches DHL rates, retries transient failures, and maps to unified response format.
    /// </summary>
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

            HttpResponseMessage? response = null;
            Exception? lastException = null;
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    response = await client.PostAsJsonAsync("/api/dhl/rates", payload, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    if (attempt < MaxAttempts && IsTransientStatusCode((int)response.StatusCode))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), cancellationToken);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt < MaxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), cancellationToken);
                        continue;
                    }
                }

                break;
            }

            if (response is null || !response.IsSuccessStatusCode)
            {
                if (lastException is not null)
                {
                    return Result<ShippingRateResponse>.Failure(
                        new Error("carrier.dhl.exception", $"DHL request failed: {lastException.Message}")
                    );
                }

                return Result<ShippingRateResponse>.Failure(
                    new Error("carrier.dhl.http_error", $"DHL API failed with {(int?)response?.StatusCode ?? 0}.")
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

    private static bool IsTransientStatusCode(int statusCode)
    {
        return statusCode == 408 || statusCode == 429 || statusCode >= 500;
    }
}
