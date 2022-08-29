namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class ExceptionValueCommitResult<TValue> : CommitResult<TValue>
{
    public ExceptionValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Exception;
    }
}
