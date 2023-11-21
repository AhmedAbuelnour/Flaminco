using System.Runtime.CompilerServices;

namespace Flaminco.Validation.Models
{
    public sealed record Error(string Code, string? Description = default, [CallerLineNumber] int SourceLineNumber = 0)
    {
        public static readonly Error None = new(string.Empty);

        public static implicit operator Result(Error error) => Result.Failure(error);
    }
}
