namespace Flaminco.OperationResults
{
    /// <summary>
    /// Encapsulates an error from any operation in the system.
    /// </summary>
    public class OperationError
    {
        /// <summary>
        /// Gets or sets the code for this error.
        /// </summary>
        /// <value>
        /// The code for this error.
        /// </value>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Gets or sets the description for this error.
        /// </summary>
        /// <value>
        /// The description for this error.
        /// </value>
        public string Description { get; set; } = default!;
    }
}
