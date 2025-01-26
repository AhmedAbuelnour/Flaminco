namespace Flaminco.StateMachine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class StateKeyAttribute(string key) : Attribute
    {
        public string Key { get; } = key;
    }
}
