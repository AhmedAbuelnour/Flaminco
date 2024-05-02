namespace Flaminco.Resultify
{
    /// <summary>
    /// Represents a collection of errors that can occur in an application. This class provides a structured way to represent multiple errors 
    /// that have occurred, offering details such as a unique code for the error collection, a description, and an array of individual errors.
    /// It also provides properties to quickly assess whether the collection represents a failure or a success state based on the presence of errors.
    /// </summary>
    public class ErrorCollection
    {
        /// <summary>
        /// Gets the unique code identifying the error collection. This code can be used to categorize the collection or for localization purposes.
        /// </summary>
        public required string Code { get; init; }

        /// <summary>
        /// Gets the description of the error collection, providing more detail about the errors contained within. This description can offer context or summarize the nature of the failures.
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// An array of Error objects, each representing an individual error within the collection. This array can be used to enumerate all errors that have occurred.
        /// </summary>
        public Error[] Errors { get; set; } = [];

        /// <summary>
        /// A read-only property that indicates whether the collection represents a failure state. It returns true if there are any errors in the collection, signifying a failure.
        /// </summary>
        public bool IsFailure { get => Errors.Length != 0; }

        /// <summary>
        /// A read-only property that indicates whether the collection represents a success state. It returns true if there are no errors in the collection, signifying success.
        /// </summary>
        public bool IsSuccess { get => !IsFailure; }
    }
}
