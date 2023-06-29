namespace Flaminco.Pipeline.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PipelineAttribute<TInput> : Attribute
{
    public required int Order { get; init; }
}
