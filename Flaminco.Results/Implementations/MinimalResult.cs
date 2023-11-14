using Flaminco.Results.Abstractions;

namespace Flaminco.Results.Implementations;

public record MinimalResult(ResultType ResultType,
                            string? ErrorCode = default,
                            string? ErrorMessage = default,
                            IDictionary<string, string[]>? ErrorDetails = default) : IMinimalResult
{
    public bool IsSuccess => ResultType == ResultType.Ok;
}

public record MinimalResult<TValue>(ResultType ResultType,
                                    TValue? Value,
                                    string? ErrorCode = default,
                                    string? ErrorMessage = default,
                                    IDictionary<string, string[]>? ErrorDetails = default) : IMinimalResult<TValue>
{
    public bool IsSuccess => ResultType == ResultType.Ok;
}

public record MinimalResults<TValue>(ResultType ResultType,
                                     IEnumerable<TValue>? Value,
                                     string? ErrorCode = default,
                                     string? ErrorMessage = default,
                                     IDictionary<string, string[]>? ErrorDetails = default) : IMinimalResults<TValue>
{
    public bool IsSuccess => ResultType == ResultType.Ok;
}


public sealed record Error(string Code, string? Description = default)
{
    public static readonly Error None = new(string.Empty);

    public static implicit operator Result(Error error) => Result.Failure(error);
}




public class Result<TReturn>
{
    protected Result(bool isSuccess, TReturn value, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public TReturn Value { get; }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result<TReturn> Success(TReturn value) => new(true, value, Error.None);

    public static Result<TReturn> Failure(Error error) => new(false, default!, error);

}

public class Result : Result<object>
{
    private Result(bool isSuccess, Error error) : base(isSuccess, new object(), error) { }

    public static Result Success() => new(true, Error.None);

    public new static Result Failure(Error error) => new(false, error);
}