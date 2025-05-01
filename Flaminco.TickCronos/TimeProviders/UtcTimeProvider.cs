namespace Flaminco.TickCronos.TimeProviders
{
    /// <summary>
    /// Provides UTC-based time functionality and access to the UTC time zone.
    /// </summary>
    public sealed class UtcTimeProvider : TimeProvider
    {
        /// <summary>
        /// Represents the UTC time zone information.
        /// </summary>
        private static readonly TimeZoneInfo _utcZone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "UTC" : "Etc/UTC");

        /// <summary>
        /// Gets the singleton instance of the <see cref="UtcTimeProvider"/> class.
        /// </summary>
        public new static UtcTimeProvider System { get; } = new();

        /// <summary>
        /// Gets the UTC time zone.
        /// </summary>
        public override TimeZoneInfo LocalTimeZone => _utcZone;
    }
}
