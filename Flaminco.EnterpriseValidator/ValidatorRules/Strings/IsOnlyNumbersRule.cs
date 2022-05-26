namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public record IsOnlyNumbersRule<T>(string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        return new ValueTask<bool>(input.All(a => char.IsDigit(a)));
    }
}

