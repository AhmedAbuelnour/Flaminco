namespace Flaminco.Workflows.Models;

public class RuleGroup
{
    public required string GroupKey { get; set; }
    public required int Order { get; set; }
    public required Rule[] Rules { get; set; }
}