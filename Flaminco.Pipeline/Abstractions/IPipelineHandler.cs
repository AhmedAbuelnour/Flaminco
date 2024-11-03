namespace Flaminco.Pipeline.Abstractions;

public interface IPipelineHandler<TValue> where TValue : class
{
    ValueTask Handler(TValue source, CancellationToken cancellationToken = default);
}