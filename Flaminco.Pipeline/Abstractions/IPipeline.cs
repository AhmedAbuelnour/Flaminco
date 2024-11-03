namespace Flaminco.Pipeline.Abstractions;

public interface IPipeline
{
    ValueTask ExecuteKeyedPipeline<TInput>(TInput source, string keyName, CancellationToken cancellationToken = default)
        where TInput : class;

    ValueTask ExecutePipeline<TInput>(TInput source, CancellationToken cancellationToken = default)
        where TInput : class;
}