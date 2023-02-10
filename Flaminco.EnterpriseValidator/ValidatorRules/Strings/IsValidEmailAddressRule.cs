using System.Text.RegularExpressions;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public partial class IsValidEmailAddressRule<T> : IValidationRule<T>
{
    public required string ValidationMessage { get; init; }

    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        if (EmailPattern().IsMatch(input))
        {
            return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }

    [GeneratedRegex("^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([azA-Z]{2,4}|[0-9]{1,3})(\\]?)$")]
    private static partial Regex EmailPattern();
}

