namespace Flaminco.StateMachine
{
    /// <summary>
    /// Attribute to define a key for a state class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class StateKeyAttribute(string key) : Attribute
    {
        /// <summary>
        /// Gets the key associated with the state.
        /// </summary>
        public string Key { get; } = key;
    }
}
