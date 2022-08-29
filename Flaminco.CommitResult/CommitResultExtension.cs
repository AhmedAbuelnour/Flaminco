using Flaminco.CommitResult.CommitResultTypes;
using Flaminco.CommitResult.ValueCommitResultsTypes;
using Flaminco.CommitResult.ValueCommitResultTypes;

namespace Flaminco.CommitResult;

public static class CommitResultExtension
{
    public static ICommitResult GetCommitResult(this ResultType resultType, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => new SuccessCommitResult(errorCode, errorMessage),
            ResultType.Empty => new EmptyCommitResult(errorCode, errorMessage),
            ResultType.Invalid => new InvalidCommitResult(errorCode, errorMessage),
            ResultType.NotFound => new NotFoundCommitResult(errorCode, errorMessage),
            ResultType.InvalidValidation => new InvalidValidationCommitResult(errorCode, errorMessage),
            ResultType.Exception => new ExceptionCommitResult(errorCode, errorMessage),
            ResultType.Duplicated => new DuplicatedCommitResult(errorCode, errorMessage),
            _ => new OthersCommitResult(errorCode, errorMessage),
        };
    }
    public static ICommitResult<T> GetValueCommitResult<T>(this ResultType resultType, T value, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => new SuccessValueCommitResult<T>(value, errorCode, errorMessage),
            ResultType.Empty => new EmptyValueCommitResult<T>(value, errorCode, errorMessage),
            ResultType.Invalid => new InvalidValueCommitResult<T>(value, errorCode, errorMessage),
            ResultType.NotFound => new NotFoundValueCommitResult<T>(value, errorCode, errorMessage),
            ResultType.InvalidValidation => new InvalidValidationValueCommitResult<T>(value, errorCode, errorMessage),
            ResultType.Exception => new ExceptionValueCommitResult<T>(value, errorCode, errorMessage),
            ResultType.Duplicated => new DuplicatedValueCommitResult<T>(value, errorCode, errorMessage),
            _ => new OthersValueCommitResult<T>(value, errorCode, errorMessage),
        };
    }
    public static ICommitResults<T> GetValueCommitResults<T>(this ResultType resultType, IEnumerable<T> value, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => new SuccessValueCommitResults<T>(value, errorCode, errorMessage),
            ResultType.Empty => new EmptyValueCommitResults<T>(value, errorCode, errorMessage),
            ResultType.Invalid => new InvalidValueCommitResults<T>(value, errorCode, errorMessage),
            ResultType.NotFound => new NotFoundValueCommitResults<T>(value, errorCode, errorMessage),
            ResultType.InvalidValidation => new InvalidValidationValueCommitResults<T>(value, errorCode, errorMessage),
            ResultType.Exception => new ExceptionValueCommitResults<T>(value, errorCode, errorMessage),
            ResultType.Duplicated => new DuplicatedValueCommitResults<T>(value, errorCode, errorMessage),
            _ => new OthersValueCommitResults<T>(value, errorCode, errorMessage),
        };
    }
}
