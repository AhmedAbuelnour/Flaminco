namespace Flaminco.Resultify
{
    /// <summary>
    /// Provides an extension method for converting a collection of Result objects into a unified ErrorCollection. 
    /// This enables the aggregation of multiple Result failures into a single ErrorCollection, simplifying error handling and presentation.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts an enumerable of Result objects into an ErrorCollection, allowing for the aggregation of errors from multiple operations.
        /// If any of the Results in the enumerable are failures, their associated errors are compiled into an ErrorCollection.
        /// This method facilitates centralized error management by consolidating errors from various sources.
        /// </summary>
        /// <param name="results">The enumerable of Result objects to convert.</param>
        /// <param name="code">A unique code representing the collective error state, used if there are any failures.</param>
        /// <param name="description">A description providing context or details about the collective error state, used if there are any failures.</param>
        /// <returns>An ErrorCollection that represents either a collection of errors if any failures are present, or an empty collection indicating success.</returns>
        public static ErrorCollection ToErrorCollection(this IEnumerable<Result> results, string code, string description)
        {
            Error[] errors = results.Where(a => a.IsFailure).Select(a => a.Error).ToArray();

            if (errors.Length == 0)
            {
                return new ErrorCollection
                {
                    Code = string.Empty,
                    Description = string.Empty,
                    Errors = []
                };
            }
            else
            {
                return new ErrorCollection
                {
                    Code = code,
                    Description = description,
                    Errors = errors
                };
            }
        }
    }
}
