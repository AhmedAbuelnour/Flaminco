namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class NotFoundValueCommitResults<TValue> : CommitResults<TValue>
{
    public NotFoundValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.NotFound;
    }
}
