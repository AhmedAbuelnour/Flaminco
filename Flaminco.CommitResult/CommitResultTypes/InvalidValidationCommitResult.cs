namespace Flaminco.CommitResult.CommitResultTypes;

public class InvalidValidationCommitResult : CommitResult
{
    public InvalidValidationCommitResult(string? errorCode, string? errorMessage) : base(errorCode, errorMessage)
    {
        ResultType = ResultType.InvalidValidation;
    }
}
