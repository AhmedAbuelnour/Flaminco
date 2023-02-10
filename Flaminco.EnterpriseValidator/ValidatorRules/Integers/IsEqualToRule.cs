﻿namespace Flaminco.EnterpriseValidator.ValidatorRules.Integers;

public class IsEqualToRule<T> : IValidationRule<T>
{
    public required string ValidationMessage { get; init; }
    public required int EqualTo { get; init; }

    public ValueTask<bool> Check(T value)
    {
        if (value == null)
            return new ValueTask<bool>(false);

        if (value is not int input)
            return new ValueTask<bool>(false);

        return new ValueTask<bool>(input.Equals(EqualTo));
    }
}

