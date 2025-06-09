namespace Flaminco.MinimalEndpoints.Options
{
    public class TimeZoneOptions
    {
        /// <summary>
        /// An ordered list of header names to check for a timezone value.
        /// </summary>
        public string[] HeaderNames { get; set; } = [];
    }
}
