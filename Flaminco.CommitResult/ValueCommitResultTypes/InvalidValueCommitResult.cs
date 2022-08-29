namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class InvalidValueCommitResult<TValue> : CommitResult<TValue>
{
    public InvalidValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Invalid;
    }
}