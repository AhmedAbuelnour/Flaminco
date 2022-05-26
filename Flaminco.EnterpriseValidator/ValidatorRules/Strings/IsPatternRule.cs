using System.Text.RegularExpressions;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;


public record IsPatternRule<T>(string Pattern, string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        if (Regex.IsMatch(input, Pattern))
        {
            return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }
}
