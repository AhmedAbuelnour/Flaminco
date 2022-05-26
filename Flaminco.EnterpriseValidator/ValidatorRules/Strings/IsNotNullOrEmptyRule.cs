namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public record IsNotNullOrEmptyRule<T>(string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        return new ValueTask<bool>(!string.IsNullOrWhiteSpace(input));
    }
}
