namespace Flaminco.CommitResult.CommitResultTypes;

public class SuccessCommitResult : CommitResult {
    public SuccessCommitResult(string? errorCode = default, string? errorMessage = default) : base(errorCode, errorMessage) {
        ResultType = ResultType.Ok;
    }
}
