namespace Flaminco.Workflows.Exceptions;

internal class BooleanEvaluationException(string expression)
    : Exception($"The expression {expression} did not evaluate to a boolean value (true or false).")
{
}