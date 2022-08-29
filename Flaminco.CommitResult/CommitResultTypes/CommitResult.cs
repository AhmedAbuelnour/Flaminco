namespace Flaminco.CommitResult.CommitResultTypes
{
    public class CommitResult : ICommitResult
    {
        public CommitResult(string? errorCode, string? errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public ResultType ResultType { get; set; }
        public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    }
}
