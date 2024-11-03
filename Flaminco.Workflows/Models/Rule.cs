namespace Flaminco.Workflows.Models;

public class Rule
{
    public required string Key { get; set; }
    public required string Expression { get; set; }
    public required int Order { get; set; }
    public OutputType OutputType { get; set; } = OutputType.Raw;
    public string? SuccessOutput { get; set; }
    public string? FailureOutput { get; set; }
    public string? ExpressionOutput { get; set; }
    public Dictionary<string, object> Inputs { get; set; } = [];
}

public enum OutputType : sbyte
{
    Raw,
    Expression
}