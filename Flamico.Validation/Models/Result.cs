namespace Flaminco.Validation.Models
{
    public class Result
    {
        private Result(bool isSuccess, IEnumerable<Error> errors)
        {
            if (isSuccess && errors.Any() || !isSuccess && !errors.Any())
            {
                throw new ArgumentException("Invalid error state", nameof(errors));
            }
            IsSuccess = isSuccess;
            Errors = new List<Error>(errors);
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public IReadOnlyList<Error> Errors { get; }

        public static Result Success() => new(true, []);

        public static Result Failure(IEnumerable<Error> errors) => new(false, errors);

        public static Result Failure(params Error[] errors) => new(false, errors);
    }
}
