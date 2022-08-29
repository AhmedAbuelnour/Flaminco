namespace Flaminco.CommitResult.CommitResultTypes;

public class NotFoundCommitResult : CommitResult
{
    public NotFoundCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.NotFound;
    }
}
