namespace Flaminco.Resultify
{
    /// <summary>
    /// Defines the types of errors that can occur during the execution of operations within an application.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Indicates a none failure in an operation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates a general failure in an operation.
        /// </summary>
        Failure = 1,

        /// <summary>
        /// Represents errors related to data validation failures.
        /// </summary>
        Validation = 2,

        /// <summary>
        /// Used when the requested resource or entity was not found.
        /// </summary>
        NotFound = 3,

        /// <summary>
        /// Signifies a conflict with the current state of the resource, such as duplicate data or edit conflicts.
        /// </summary>
        Conflict = 4
    }
}
