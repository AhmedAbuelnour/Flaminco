namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class SuccessValueCommitResult<TValue> : CommitResult<TValue>
{
    public SuccessValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Ok;
    }
}