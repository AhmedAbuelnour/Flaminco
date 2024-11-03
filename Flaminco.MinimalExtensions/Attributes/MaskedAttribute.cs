namespace LowCodeHub.MinimalExtensions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class MaskedAttribute(int start, int length, char maskingChar = '*') : Attribute
{
    public int Start { get; private set; } = start;
    public int Length { get; private set; } = length;
    public char MaskingChar { get; private set; } = maskingChar;
}