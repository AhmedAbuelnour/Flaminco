namespace Flaminco.EnterpriseValidator.ValidatorRules.Integers;

public record IsNotEqualToRule<T>(int NotEqualTo, string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not int input)
            return new ValueTask<bool>(false);

        return new ValueTask<bool>(!input.Equals(NotEqualTo));
    }
}

