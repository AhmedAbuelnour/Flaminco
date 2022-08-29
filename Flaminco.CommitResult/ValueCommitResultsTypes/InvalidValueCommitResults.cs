namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class InvalidValueCommitResults<TValue> : CommitResults<TValue>
{
    public InvalidValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Invalid;
    }
}