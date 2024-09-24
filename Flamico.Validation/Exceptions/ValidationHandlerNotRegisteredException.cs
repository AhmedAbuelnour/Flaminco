namespace Flaminco.Validation.Exceptions;

/// <summary>
/// Exception thrown when a validation handler is not registered.
/// </summary>
/// <typeparam name="TValidation">The type of the validation handler that was not registered.</typeparam>
internal sealed class ValidationHandlerNotRegisteredException<TValidation> : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationHandlerNotRegisteredException{TValidation}"/> class.
    /// </summary>
    public ValidationHandlerNotRegisteredException()
        : base($"Validation Handler {typeof(TValidation).Name} Not Registered!")
    {
    }
}
