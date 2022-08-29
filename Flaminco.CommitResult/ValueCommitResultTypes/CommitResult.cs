namespace Flaminco.CommitResult.ValueCommitResultTypes
{
    public class CommitResult<TValue> : ICommitResult<TValue>
    {
        public CommitResult(TValue? value, string? errorCode, string? errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Value = value;
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public ResultType ResultType { get; set; } = ResultType.Empty;
        public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
        public TValue? Value { get; set; }
    }
}
