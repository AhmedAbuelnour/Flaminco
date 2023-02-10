using System.Text.RegularExpressions;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public partial class IsGUIDRule<T> : IValidationRule<T>
{
    public required string ValidationMessage { get; init; }
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        if (GUIDPattern().IsMatch(input))
        {
            return new ValueTask<bool>(true);
        }
        return new ValueTask<bool>(false);
    }

    [GeneratedRegex("^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$")]
    private static partial Regex GUIDPattern();
}
