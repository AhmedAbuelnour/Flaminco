namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class OthersValueCommitResults<TValue> : CommitResults<TValue>
{
    public OthersValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Others;
    }
}
