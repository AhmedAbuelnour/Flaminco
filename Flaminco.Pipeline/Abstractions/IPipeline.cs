namespace Flaminco.Pipeline.Abstractions;

public interface IPipeline
{
    ValueTask ExecutePipeline<TInput>(TInput source, CancellationToken cancellationToken = default) where TInput : class;
}
