namespace Flaminco.TypedResults.Abstractions;

public sealed record SuccessResult() : TypedResult(true) { }

public sealed record SuccessResult<TValue>(TValue Value) : TypedResult(true) { }

public sealed record SuccessResults<TValue>(IEnumerable<TValue> Value) : TypedResult(true) { }
