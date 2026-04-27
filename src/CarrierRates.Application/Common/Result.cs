namespace CarrierRates.Application.Common;

public class Result
{
    protected readonly List<Error> _errors = [];
    protected readonly List<string> _warnings = [];

    public bool IsSuccess => _errors.Count == 0;
    public IReadOnlyCollection<Error> Errors => _errors;
    public IReadOnlyCollection<string> Warnings => _warnings;

    public static Result Success(IEnumerable<string>? warnings = null)
    {
        var result = new Result();
        if (warnings is not null)
        {
            result._warnings.AddRange(warnings);
        }

        return result;
    }

    public static Result Failure(params Error[] errors)
    {
        var result = new Result();
        result._errors.AddRange(errors);
        return result;
    }
}

public sealed class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value, IEnumerable<string>? warnings = null)
    {
        var result = new Result<T> { Value = value };
        if (warnings is not null)
        {
            result._warnings.AddRange(warnings);
        }

        return result;
    }

    public static new Result<T> Failure(params Error[] errors)
    {
        var result = new Result<T>();
        result._errors.AddRange(errors);
        return result;
    }
}
