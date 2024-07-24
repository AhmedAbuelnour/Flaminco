using System.Globalization;

namespace Flaminco.OperationResults
{
    /// <summary>
    /// Represents the result of an operation.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; init; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="OperationError"/> instances containing errors
        /// that occurred during the operation.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of <see cref="OperationError"/> instances.</value>
        public IEnumerable<OperationError> Errors { get; set; } = [];

        /// <summary>
        /// Returns an <see cref="OperationResult"/> indicating a successful operation.
        /// </summary>
        /// <returns>An <see cref="OperationResult"/> indicating a successful operation.</returns>
        public static OperationResult Success => new() { Succeeded = true };

        /// <summary>
        /// Creates an <see cref="OperationResult"/> indicating a failed operation, with a list of <paramref name="errors"/> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="OperationError"/>s which caused the operation to fail.</param>
        /// <returns>An <see cref="OperationResult"/> indicating a failed operation, with a list of <paramref name="errors"/> if applicable.</returns>
        public static OperationResult Failed(params OperationError[] errors)
        {
            return new OperationResult
            {
                Succeeded = false,
                Errors = errors ?? []
            };
        }

        /// <summary>
        /// Converts the value of the current <see cref="OperationResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="OperationResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Succeeded ?
                   "Succeeded" :
                   string.Format(CultureInfo.InvariantCulture, "{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.Code).ToList()));
        }

    }

    /// <summary>
    /// Represents the result of an operation with a result value of type <typeparamref name="TResult"/>.
    /// Inherits from <see cref="OperationResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    public class OperationResult<TResult> : OperationResult
    {

        /// <summary>
        /// Gets the result value of the operation.
        /// </summary>
        /// <value>The result value of type <typeparamref name="TResult"/>.</value>
        public TResult? Result { get; init; }

        /// <summary>
        /// Creates a successful <see cref="OperationResult{TResult}"/> with the specified result value.
        /// </summary>
        /// <param name="value">The result value of the operation.</param>
        /// <returns>An <see cref="OperationResult{TResult}"/> indicating a successful operation with the specified result value.</returns>
        public static OperationResult<TResult> Succeed(TResult? value)
        {
            return new OperationResult<TResult>()
            {
                Succeeded = true,
                Result = value,
                Errors = []
            };
        }
    }
}
