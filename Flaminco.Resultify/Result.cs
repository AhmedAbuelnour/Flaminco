using System.Diagnostics.CodeAnalysis;

namespace Flaminco.Resultify
{
    /// <summary>
    /// Represents the outcome of an operation, encapsulating both success and failure states. In case of failure, an error detailing the reason is included.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Initializes a new instance of the Result class, ensuring consistency between the success state and the associated error.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
        /// <param name="error">The error associated with a failed operation. Should be Error.None for successful operations.</param>
        /// <exception cref="ArgumentException">Thrown when the error state is inconsistent with the success flag.</exception>
        protected internal Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None ||
                !isSuccess && error == Error.None)
            {
                throw new ArgumentException("Invalid error", nameof(error));
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error associated with a failed operation. Is Error.None for successful operations.
        /// </summary>
        public Error Error { get; }

        /// <summary>
        /// Creates a success result with no associated value.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success() => new(true, Error.None);

        /// <summary>
        /// Creates a success result with an associated value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value associated with a successful operation.</typeparam>
        /// <param name="value">The value to associate with the successful result.</param>
        /// <returns>A successful result containing the specified value.</returns>
        public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

        /// <summary>
        /// Creates a failure result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failure.</param>
        /// <returns>A failed result with the specified error.</returns>
        public static Result Failure(Error error) => new(false, error);

        /// <summary>
        /// Creates a failure result with the specified error and without an associated value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value that would have been associated with a successful operation.</typeparam>
        /// <param name="error">The error associated with the failure.</param>
        /// <returns>A failed result with the specified error.</returns>
        public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
    }

    /// <summary>
    /// Represents the outcome of an operation that returns a value on success. Inherits from the non-generic Result class to include an associated value on success.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the operation.</typeparam>
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        /// <summary>
        /// Initializes a new instance of the Result class with an associated value, ensuring consistency between the success state, the value, and the associated error.
        /// </summary>
        /// <param name="value">The value associated with a successful operation.</param>
        /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
        /// <param name="error">The error associated with a failed operation. Should be Error.None for successful operations.</param>
        protected internal Result(TValue? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value associated with a successful operation. Throws an InvalidOperationException if accessed on a failure result.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
        [NotNull]
        public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result can't be accessed.");

        /// <summary>
        /// Provides an implicit conversion from a value to a Result&lt;TValue&gt;, allowing a value to be used directly where a Result&lt;TValue&gt; is expected.
        /// </summary>
        /// <param name="value">The value to convert into a successful Result&lt;TValue&gt;.</param>
        /// <returns>A successful Result&lt;TValue&gt; containing the specified value.</returns>
        public static implicit operator Result<TValue>(TValue value) => Success(value);
    }
}
