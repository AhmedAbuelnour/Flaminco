namespace Flaminco.Workflows.Exceptions;

internal class EmptyRulesException(string workflow) : Exception($"Workflow {workflow} can't have empty rules.")
{
}