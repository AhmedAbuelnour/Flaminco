namespace Flaminco.CommitResult.CommitResultTypes;

public class SuccessCommitResult : CommitResult
{
    public SuccessCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.Ok;
    }
}
