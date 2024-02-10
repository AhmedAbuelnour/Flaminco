namespace Flaminco.Validation.Exceptions
{
    public class ValidationHandlerNotRegisteredException<TValidation> : Exception
    {
        public ValidationHandlerNotRegisteredException() : base($"Validation Handler {typeof(TValidation).Name} Not Registered!")
        {

        }
    }
}
