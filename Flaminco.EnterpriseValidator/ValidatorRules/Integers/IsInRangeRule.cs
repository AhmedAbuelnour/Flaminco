﻿namespace Flaminco.EnterpriseValidator.ValidatorRules.Integers;

public record IsInRangeRule<T>(int Start, int End, string ValidationMessage) : IValidationRule<T>
{
    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not int input)
            return new ValueTask<bool>(false);

        return new ValueTask<bool>(input >= Start && input <= End);
    }
}

