namespace Flaminco.AuditTrail.Core.Tracker;


[AttributeUsage(AttributeTargets.Class)]
public class TableNameAttribute : Attribute
{
    private string _Name;
    public TableNameAttribute(string TableName)
    {
        _Name = TableName;
    }
    public string Name => _Name;
}
