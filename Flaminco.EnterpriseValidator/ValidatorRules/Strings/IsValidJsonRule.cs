using System.Text.Json;

namespace Flaminco.EnterpriseValidator.ValidatorRules.Strings;

public record IsValidJsonRule<T>(string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not string input)
            return new ValueTask<bool>(false);

        try
        {
            JsonDocument.Parse(input);
            return new ValueTask<bool>(true);
        }
        catch (JsonException)
        {
            return new ValueTask<bool>(false);
        }
    }
}

