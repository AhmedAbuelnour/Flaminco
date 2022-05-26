using System.Text.RegularExpressions;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public record IsGuidRule<T>(string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        string pattern = @"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$";
        if (Regex.IsMatch(input, pattern))
        {
            if (Regex.Replace(input, pattern, string.Empty).Length == 0)
            {
                return new ValueTask<bool>(true);
            }
        }
        return new ValueTask<bool>(false);
    }
}
