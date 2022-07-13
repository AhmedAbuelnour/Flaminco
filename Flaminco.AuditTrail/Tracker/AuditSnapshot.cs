namespace Flaminco.AuditTrail.Core.Tracker;

public class AuditSnapshot
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = default!;
    public string? PKValue { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Changes { get; set; }
    public DateTime CreatedOn { get; set; }
    public ActionType ActionType { get; set; }
    public string? UserId { get; set; }
}

public enum ActionType
{
    Add,
    Update,
    Delete,
}