namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class EmptyValueCommitResult<TValue> : CommitResult<TValue>
{
    public EmptyValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Empty;
    }
}