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
            ResultType.Ok => new SuccessCommitResult(),
            ResultType.Empty => new EmptyCommitResult(errorCode, errorMessage),
            ResultType.Invalid => new InvalidCommitResult(errorCode, errorMessage),
            ResultType.NotFound => new NotFoundCommitResult(errorCode, errorMessage),
            ResultType.InvalidValidation => new InvalidCommitResult(errorCode, errorMessage),
            ResultType.Exception => new ExceptionCommitResult(errorCode, errorMessage),
            ResultType.Duplicated => new DuplicatedCommitResult(errorCode, errorMessage),
            _ => new OthersCommitResult(errorCode, errorMessage),
        };
    }
    public static ICommitResult<T> GetValueCommitResult<T>(this ResultType resultType, T value, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => new SuccessValueCommitResult<T>(value),
            ResultType.Empty => new EmptyValueCommitResult<T>(errorCode, errorMessage),
            ResultType.Invalid => new InvalidValueCommitResult<T>(errorCode, errorMessage),
            ResultType.NotFound => new NotFoundValueCommitResult<T>(errorCode, errorMessage),
            ResultType.InvalidValidation => new InvalidValueCommitResult<T>(errorCode, errorMessage),
            ResultType.Exception => new ExceptionValueCommitResult<T>(errorCode, errorMessage),
            ResultType.Duplicated => new DuplicatedValueCommitResult<T>(errorCode, errorMessage),
            _ => new OthersValueCommitResult<T>(errorCode, errorMessage),
        };
    }
    public static ICommitResults<T> GetValueCommitResults<T>(this ResultType resultType, IEnumerable<T> value, string? errorCode = default, string? errorMessage = default)
    {
        return resultType switch
        {
            ResultType.Ok => new SuccessValueCommitResults<T>(value),
            ResultType.Empty => new EmptyValueCommitResults<T>(errorCode, errorMessage),
            ResultType.Invalid => new InvalidValueCommitResults<T>(errorCode, errorMessage),
            ResultType.NotFound => new NotFoundValueCommitResults<T>(errorCode, errorMessage),
            ResultType.InvalidValidation => new InvalidValueCommitResults<T>(errorCode, errorMessage),
            ResultType.Exception => new ExceptionValueCommitResults<T>(errorCode, errorMessage),
            ResultType.Duplicated => new DuplicatedValueCommitResults<T>(errorCode, errorMessage),
            _ => new OthersValueCommitResults<T>(errorCode, errorMessage),
        };
    }
}
