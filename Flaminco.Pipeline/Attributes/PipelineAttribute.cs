namespace Flaminco.Pipeline.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PipelineAttribute<TInput> : Attribute
{
    public required int Order { get; init; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class KeyedPipelineAttribute<TInput> : Attribute
{
    public required int Order { get; init; }
    public required string Key { get; set; }
}