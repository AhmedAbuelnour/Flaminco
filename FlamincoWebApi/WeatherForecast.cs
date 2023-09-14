using Flaminco.ManualMapper.Abstractions;

namespace FlamincoWebApi
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }

    public class Person
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PersonResponse : IMapHandler<Person, PersonResponse>
    {
        public int Age { get; set; }
        public string FullName { get; set; }

        public PersonResponse Handler(Person source)
        {
            return new PersonResponse
            {
                Age = source.Age,
                FullName = $"{source.FirstName} {source.LastName}"
            };
        }
    }

    public class PersonMapper : IMapHandler<Person, PersonResponse>
    {
        public PersonResponse Handler(Person source)
        {
            return new PersonResponse
            {
                Age = source.Age,
                FullName = $"{source.FirstName} {source.LastName}"
            };
        }
    }
}