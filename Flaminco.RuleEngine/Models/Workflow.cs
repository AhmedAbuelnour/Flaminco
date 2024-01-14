namespace Flaminco.RuleEngine.Models
{
    public class Workflow
    {
        public required string Name { get; set; }
        public required WorkflowType WorkflowType { get; set; }
        public Rule[]? Rules { get; set; }
        public RuleGroup[]? RuleGroups { get; set; }
    }

    public enum WorkflowType : sbyte
    {
        Rules = 1,
        Groups
    }
}
