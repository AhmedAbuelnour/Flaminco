namespace Flaminco.MinimalEndpoints.Models
{
    public sealed class ValidationFailure
    {
        public string PropertyName { get; set; } = default!;
        public string ErrorMessage { get; set; } = default!;
        public string? ErrorCode { get; set; }
        public object? AttemptedValue { get; set; }
    }
}
