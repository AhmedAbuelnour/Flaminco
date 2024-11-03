namespace Flaminco.Results.Abstractions;

public interface IMinimalResult
{
    string? ErrorMessage { get; init; }
    IDictionary<string, string[]>? ErrorDetails { get; init; }
    string? ErrorCode { get; init; }
    ResultType ResultType { get; init; }
    bool IsSuccess { get; }
}

public interface IMinimalResult<T> : IMinimalResult
{
    T? Value { get; init; }
}

public interface IMinimalResults<T> : IMinimalResult
{
    IEnumerable<T>? Value { get; init; }
}