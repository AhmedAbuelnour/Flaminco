namespace Flaminco.Pipeline.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PipelineAttribute : Attribute
{
    public required int Order { get; init; }
    public required string Name { get; init; }
}
