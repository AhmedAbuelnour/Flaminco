namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class NotFoundValueCommitResult<TValue> : CommitResult<TValue>
{
    public NotFoundValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.NotFound;
    }
}