namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class InvalidValidationValueCommitResult<TValue> : CommitResult<TValue>
{
    public InvalidValidationValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.InvalidValidation;
    }
}