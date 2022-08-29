namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class ExceptionValueCommitResults<TValue> : CommitResults<TValue>
{
    public ExceptionValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Exception;
    }
}