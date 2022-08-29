namespace Flaminco.CommitResult.CommitResultTypes;

public class OthersCommitResult : CommitResult
{
    public OthersCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.Others;
    }
}