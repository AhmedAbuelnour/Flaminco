namespace LowCodeHub.Operations;

/// <summary>
/// Used as request type when an operation has no request (ExecuteAsync(CancellationToken) only).
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = default;
}
