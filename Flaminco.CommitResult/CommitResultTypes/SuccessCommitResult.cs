namespace Flaminco.CommitResult.CommitResultTypes;

public class SuccessCommitResult : ICommitResult
{
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Ok;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
}
