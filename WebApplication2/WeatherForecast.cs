using LowCodeHub.MinimalExtensions.Attributes;

namespace WebApplication2
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [Masked(1, 3)]
        public string? Summary { get; set; }
    }

    public class WeatherForecast2 : WeatherForecast
    {
        public string TestName { get; set; }
    }
}
