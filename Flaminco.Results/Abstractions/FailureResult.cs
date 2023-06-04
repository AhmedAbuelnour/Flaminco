using Flaminco.Results;

namespace Flaminco.TypedResults.Abstractions;

public sealed record FailureResult(FailureType ResultType, string? ErrorCode = default, string? ErrorMessage = default) : TypedResult(false) { }

public sealed record FailureResult<TValue>(FailureType ResultType, TValue? Value = default, string? ErrorCode = default, string? ErrorMessage = default) : TypedResult(false) { }

public sealed record FailureResults<TValue>(FailureType ResultType, IEnumerable<TValue>? Value = default, string? ErrorCode = default, string? ErrorMessage = default) : TypedResult(false) { }
