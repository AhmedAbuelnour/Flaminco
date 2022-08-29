namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class DuplicatedValueCommitResults<TValue> : CommitResults<TValue>
{
    public DuplicatedValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Duplicated;
    }
}
