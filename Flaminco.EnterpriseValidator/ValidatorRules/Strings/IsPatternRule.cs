using System.Text.RegularExpressions;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public class IsPatternRule<T> : IValidationRule<T>
{
    public required string ValidationMessage { get; init; }
    public required string Pattern { get; init; }

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
