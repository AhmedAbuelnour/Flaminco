namespace Flaminco.EnterpriseValidator;


/// <summary>
/// An interface for defining a validation rule requirements
/// </summary>
/// <typeparam name="T">
/// A datatype that matches the datatype that you want to run a validation rule against
/// </typeparam>
public interface IValidationRule<T>
{
    string ValidationMessage { get; init; }
    ValueTask<bool> Check(T value);
}

