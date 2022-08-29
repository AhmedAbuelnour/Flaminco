namespace Flaminco.CommitResult.CommitResultTypes;

public class ExceptionCommitResult : CommitResult
{
    public ExceptionCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.Exception;
    }
}
