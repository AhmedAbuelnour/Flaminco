namespace Flaminco.Pipeline.Abstractions;

public interface IPipeline
{
    ValueTask ExecutePipeline<TValue>(TValue source, string name, CancellationToken cancellationToken = default) where TValue : class;
}
