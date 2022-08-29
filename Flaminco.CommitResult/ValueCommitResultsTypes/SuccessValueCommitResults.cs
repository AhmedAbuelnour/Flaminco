namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class SuccessValueCommitResults<TValue> : CommitResults<TValue>
{
    public SuccessValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Ok;
    }
}
