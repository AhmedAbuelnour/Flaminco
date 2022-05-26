using System.Text.RegularExpressions;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public record IsValidEmailAddressRule<T>(string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([azA-Z]{2,4}|[0-9]{1,3})(\]?)$";
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

