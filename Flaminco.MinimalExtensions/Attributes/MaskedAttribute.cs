namespace LowCodeHub.MinimalExtensions.Attributes
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaskedAttribute : Attribute
    {
        public int Start { get; private set; }
        public int Length { get; private set; }
        public char MaskingChar { get; private set; }

        public MaskedAttribute(int start, int length, char maskingChar = '*')
        {
            Start = start;
            Length = length;
            MaskingChar = maskingChar;
        }
    }
}
