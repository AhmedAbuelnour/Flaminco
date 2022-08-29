namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class OthersValueCommitResult<TValue> : CommitResult<TValue>
{
    public OthersValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Others;
    }
}