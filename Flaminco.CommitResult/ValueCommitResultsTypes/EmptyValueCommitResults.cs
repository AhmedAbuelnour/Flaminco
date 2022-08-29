namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class EmptyValueCommitResults<TValue> : CommitResults<TValue>
{
    public EmptyValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Empty;
    }
}