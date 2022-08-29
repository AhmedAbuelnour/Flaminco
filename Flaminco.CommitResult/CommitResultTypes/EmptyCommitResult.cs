namespace Flaminco.CommitResult.CommitResultTypes;

public class EmptyCommitResult : CommitResult
{
    public EmptyCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.Empty;
    }
}
