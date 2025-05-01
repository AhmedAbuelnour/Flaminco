namespace Flaminco.TickCronos.TimeProviders
{
    /// <summary>
    /// Provides the local time zone information for Saudi Arabia.
    /// </summary>
    public sealed class SaudiTimeProvider : TimeProvider
    {
        /// <summary>
        /// Represents the Saudi time zone information.
        /// </summary>
        private static readonly TimeZoneInfo _saudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "Arab Standard Time" : "Asia/Riyadh");

        /// <summary>
        /// Gets the singleton instance of the <see cref="SaudiTimeProvider"/> class.
        /// </summary>
        public new static SaudiTimeProvider System { get; } = new();

        /// <summary>
        /// Gets the local time zone for Saudi Arabia.
        /// </summary>
        public override TimeZoneInfo LocalTimeZone => _saudiTimeZone;
    }
}
