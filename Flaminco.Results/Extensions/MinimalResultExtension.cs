using Flaminco.Results.Implementations;
using Microsoft.AspNetCore.Http;

namespace Flaminco.Results.Extensions;

public static class MinimalResultExtension
{
    public static IResult GetMinimalResult(this ResultType resultType,
        string? errorCode = default,
        string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => TypedResults.Ok(new MinimalResult(ResultType.Ok, errorCode, errorMessage)),
            ResultType.NotFound => TypedResults.NotFound(
                new MinimalResult(ResultType.NotFound, errorCode, errorMessage)),
            ResultType.Conflict => TypedResults.Conflict(
                new MinimalResult(ResultType.Conflict, errorCode, errorMessage)),
            ResultType.BadRequest => TypedResults.BadRequest(new MinimalResult(ResultType.BadRequest, errorCode,
                errorMessage)),
            ResultType.UnprocessableEntity => TypedResults.UnprocessableEntity(
                new MinimalResult(ResultType.UnprocessableEntity, errorCode, errorMessage)),
            _ => TypedResults.Empty
        };
    }

    public static IResult GetMinimalResult(this ResultType resultType, IDictionary<string, string[]> errorDetails)
    {
        return resultType switch
        {
            ResultType.Ok => TypedResults.Ok(new MinimalResult(ResultType.Ok, default, default, errorDetails)),
            ResultType.NotFound => TypedResults.NotFound(new MinimalResult(ResultType.NotFound, default, default,
                errorDetails)),
            ResultType.Conflict => TypedResults.Conflict(new MinimalResult(ResultType.Conflict, default, default,
                errorDetails)),
            ResultType.BadRequest => TypedResults.BadRequest(new MinimalResult(ResultType.BadRequest, default, default,
                errorDetails)),
            ResultType.UnprocessableEntity => TypedResults.UnprocessableEntity(
                new MinimalResult(ResultType.UnprocessableEntity, default, default, errorDetails)),
            _ => TypedResults.Empty
        };
    }

    public static IResult GetMinimalResult<TValue>(this ResultType resultType,
        TValue value,
        string? errorCode = default,
        string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => TypedResults.Ok(new MinimalResult<TValue>(ResultType.Ok, value, errorCode, errorMessage)),
            ResultType.NotFound => TypedResults.NotFound(new MinimalResult<TValue>(ResultType.NotFound, value,
                errorCode, errorMessage)),
            ResultType.Conflict => TypedResults.Conflict(new MinimalResult<TValue>(ResultType.Conflict, value,
                errorCode, errorMessage)),
            ResultType.BadRequest => TypedResults.BadRequest(new MinimalResult<TValue>(ResultType.BadRequest, value,
                errorCode, errorMessage)),
            ResultType.UnprocessableEntity => TypedResults.UnprocessableEntity(
                new MinimalResult<TValue>(ResultType.UnprocessableEntity, value, errorCode, errorMessage)),
            _ => TypedResults.Empty
        };
    }


    public static IResult GetMinimalResults<TValue>(this ResultType resultType,
        IEnumerable<TValue> value,
        string? errorCode = default,
        string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => TypedResults.Ok(new MinimalResults<TValue>(ResultType.Ok, value, errorCode, errorMessage)),
            ResultType.NotFound => TypedResults.NotFound(new MinimalResults<TValue>(ResultType.NotFound, value,
                errorCode, errorMessage)),
            ResultType.Conflict => TypedResults.Conflict(new MinimalResults<TValue>(ResultType.Conflict, value,
                errorCode, errorMessage)),
            ResultType.BadRequest => TypedResults.BadRequest(new MinimalResults<TValue>(ResultType.BadRequest, value,
                errorCode, errorMessage)),
            ResultType.UnprocessableEntity => TypedResults.UnprocessableEntity(
                new MinimalResults<TValue>(ResultType.UnprocessableEntity, value, errorCode, errorMessage)),
            _ => TypedResults.Empty
        };
    }
}