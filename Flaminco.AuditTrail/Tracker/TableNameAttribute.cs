namespace Flaminco.AuditTrail.Core.Tracker;


[AttributeUsage(AttributeTargets.Class)]
public class TableNameAttribute : Attribute
{
    private string _name;
    public TableNameAttribute(string TableName)
    {
        _name = TableName;
    }
    public string Name => _name;
}
