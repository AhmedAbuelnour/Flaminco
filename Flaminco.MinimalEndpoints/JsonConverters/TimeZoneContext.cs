namespace Flaminco.MinimalEndpoints.JsonConverters
{
    internal sealed class TimeZoneContext
    {
        private static readonly AsyncLocal<TimeZoneInfo?> _zone = new();
        public static TimeZoneInfo Zone
        {
            get => _zone.Value ?? TimeZoneInfo.Utc;
            set => _zone.Value = value;
        }
    }
}
