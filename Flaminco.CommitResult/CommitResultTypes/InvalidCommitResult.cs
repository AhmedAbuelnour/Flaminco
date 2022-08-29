namespace Flaminco.CommitResult.CommitResultTypes;

public class InvalidCommitResult : CommitResult
{
    public InvalidCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.Invalid;
    }
}
