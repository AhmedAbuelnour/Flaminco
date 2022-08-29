namespace Flaminco.CommitResult.CommitResultTypes;

public class DuplicatedCommitResult : CommitResult
{
    public DuplicatedCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.Duplicated;
    }
}
