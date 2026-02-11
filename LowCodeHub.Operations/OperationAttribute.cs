namespace LowCodeHub.Operations;

/// <summary>
/// Marks a partial class as an operation. The source generator will generate a short-named
/// interface for injection. Optionally specify the emitted interface name.
/// </summary>
/// <param name="interfaceName">Optional. The name of the generated interface (e.g. "IUploadAttachmentOperation"). If not set, defaults to I{ClassName} or I{ClassName}Operation.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class OperationAttribute(string? interfaceName = null) : Attribute
{
    /// <summary>
    /// The name of the generated interface when specified; otherwise the generator uses the default convention.
    /// </summary>
    public string? InterfaceName { get; } = interfaceName;
}
