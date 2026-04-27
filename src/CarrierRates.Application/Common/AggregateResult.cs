namespace CarrierRates.Application.Common;

public sealed class AggregateResult<T>
{
    public required T Value { get; init; }
    public bool IsSuccess { get; init; }
    public IReadOnlyCollection<CarrierError> CarrierErrors { get; init; } = [];
    public IReadOnlyCollection<string> Warnings { get; init; } = [];
}
