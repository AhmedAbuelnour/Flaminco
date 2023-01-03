using Flaminco.CommitResult;
using Flaminco.CommitResult.CommitResultTypes;
using Flaminco.CommitResult.ValueCommitResultsTypes;
using Flaminco.CommitResult.ValueCommitResultTypes;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalMediatR.Extensions;

public static class CommitResultExtension
{
    public static IResult GetMinimalCommitResult(this ResultType resultType, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => Results.Ok(new SuccessCommitResult(errorCode, errorMessage)),
            ResultType.Empty => Results.NoContent(),
            ResultType.Invalid => Results.BadRequest(new InvalidCommitResult(errorCode, errorMessage)),
            ResultType.NotFound => Results.NotFound(new NotFoundCommitResult(errorCode, errorMessage)),
            ResultType.InvalidValidation => Results.BadRequest(new InvalidValidationCommitResult(errorCode, errorMessage)),
            ResultType.Exception => Results.BadRequest(new ExceptionCommitResult(errorCode, errorMessage)),
            ResultType.Duplicated => Results.Ok(new DuplicatedCommitResult(errorCode, errorMessage)),
            _ => Results.Ok(new OthersCommitResult(errorCode, errorMessage)),
        };
    }
    public static IResult GetMinimalValueCommitResult<T>(this ResultType resultType, T value, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => Results.Ok(new SuccessValueCommitResult<T>(value, errorCode, errorMessage)),
            ResultType.Empty => Results.NoContent(),
            ResultType.Invalid => Results.BadRequest(new InvalidValueCommitResult<T>(value, errorCode, errorMessage)),
            ResultType.NotFound => Results.NotFound(new NotFoundValueCommitResult<T>(value, errorCode, errorMessage)),
            ResultType.InvalidValidation => Results.BadRequest(new InvalidValidationValueCommitResult<T>(value, errorCode, errorMessage)),
            ResultType.Exception => Results.BadRequest(new ExceptionValueCommitResult<T>(value, errorCode, errorMessage)),
            ResultType.Duplicated => Results.Ok(new DuplicatedValueCommitResult<T>(value, errorCode, errorMessage)),
            _ => Results.Ok(new OthersValueCommitResult<T>(value, errorCode, errorMessage))
        };
    }
    public static IResult GetMinimalValueCommitResults<T>(this ResultType resultType, IEnumerable<T> value, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => Results.Ok(new SuccessValueCommitResults<T>(value, errorCode, errorMessage)),
            ResultType.Empty => Results.NoContent(),
            ResultType.Invalid => Results.BadRequest(new InvalidValueCommitResults<T>(value, errorCode, errorMessage)),
            ResultType.NotFound => Results.NotFound(new NotFoundValueCommitResults<T>(value, errorCode, errorMessage)),
            ResultType.InvalidValidation => Results.BadRequest(new InvalidValidationValueCommitResults<T>(value, errorCode, errorMessage)),
            ResultType.Exception => Results.BadRequest(new ExceptionValueCommitResults<T>(value, errorCode, errorMessage)),
            ResultType.Duplicated => Results.Ok(new DuplicatedValueCommitResults<T>(value, errorCode, errorMessage)),
            _ => Results.Ok(new OthersValueCommitResults<T>(value, errorCode, errorMessage))
        };
    }
}
