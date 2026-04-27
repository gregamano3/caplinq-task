using CarrierRates.Application.Abstractions.Carriers;
using CarrierRates.Application.Abstractions.Persistence;
using CarrierRates.Application.Common;
using CarrierRates.Domain.Models;

namespace CarrierRates.Infrastructure.Carriers;

public class CarrierRateAggregator(
    IEnumerable<ICarrierRateStrategy> strategies,
    ICarrierConfigRepository carrierConfigRepository
) : ICarrierRateAggregator
{
    public async Task<AggregateResult<IReadOnlyCollection<ShippingRateResponse>>> QueryRatesAsync(
        RateQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var enabledCarriers = await carrierConfigRepository.GetAllEnabledAsync(cancellationToken);
        var tasks = new List<Task<(string CarrierKey, Result<ShippingRateResponse> Result)>>();

        foreach (var carrier in enabledCarriers)
        {
            var strategy = strategies.FirstOrDefault(x => x.CanHandle(carrier.CarrierKey));
            if (strategy is null)
            {
                tasks.Add(Task.FromResult((
                    carrier.CarrierKey,
                    Result<ShippingRateResponse>.Failure(
                        new Error("carrier.strategy_missing", $"No strategy found for {carrier.CarrierKey}.", carrier.CarrierKey)
                    )
                )));
                continue;
            }

            tasks.Add(WrapAsync(carrier.CarrierKey, strategy, query, cancellationToken));
        }

        var results = await Task.WhenAll(tasks);

        var successful = results.Where(x => x.Result.IsSuccess && x.Result.Value is not null)
            .Select(x => x.Result.Value!)
            .ToArray();

        var carrierErrors = results.Where(x => !x.Result.IsSuccess)
            .SelectMany(
                x => x.Result.Errors.Select(err => new CarrierError(x.CarrierKey, err.Code, err.Message))
            )
            .ToArray();

        return new AggregateResult<IReadOnlyCollection<ShippingRateResponse>>
        {
            IsSuccess = successful.Length > 0,
            Value = successful,
            CarrierErrors = carrierErrors,
            Warnings = carrierErrors.Select(x => x.Message).ToArray()
        };
    }

    private static async Task<(string CarrierKey, Result<ShippingRateResponse> Result)> WrapAsync(
        string carrierKey,
        ICarrierRateStrategy strategy,
        RateQuery query,
        CancellationToken cancellationToken
    )
    {
        var result = await strategy.GetRatesAsync(query, cancellationToken);
        return (carrierKey, result);
    }
}
