namespace Flaminco.CommitResult;

public interface ICommitResult
{
    string? ErrorMessage { get; set; }
    string? ErrorCode { get; set; }
    ResultType ResultType { get; set; }
    bool IsSuccess { get; }
}
public class CommitResult : ICommitResult
{
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; }
    public bool IsSuccess { get; }
}


public interface ICommitResult<T> : ICommitResult
{
    T? Value { get; set; }
}

public class CommitResult<T> : ICommitResult<T>
{
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; }
    public bool IsSuccess { get; }
    public T? Value { get; set; }
}

public interface ICommitResults<T> : ICommitResult
{
    IEnumerable<T>? Value { get; set; }
}

public class CommitResults<T> : ICommitResults<T>
{
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; }
    public bool IsSuccess { get; }
    public IEnumerable<T>? Value { get; set; }
}
