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
