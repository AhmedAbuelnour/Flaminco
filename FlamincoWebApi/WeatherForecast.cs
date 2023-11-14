using Flaminco.ManualMapper.Abstractions;
using Microsoft.AspNetCore.HttpLogging;

namespace FlamincoWebApi
{
    public class HttpLoggerInterceptor : IHttpLoggingInterceptor
    {
        public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
        {
            logContext.AddParameter("Username", "ahmed abuelnour");

            return ValueTask.CompletedTask;
        }

        public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
        {
            if (logContext.IsAnyEnabled(HttpLoggingFields.Response))
            {
                logContext.LoggingFields = HttpLoggingFields.All;

                logContext.AddParameter("Username Response", "ahmed abuelnour XXX");
            }

            return ValueTask.CompletedTask;
        }
    }


    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }

        public void Bind<T>(T value)
        {
            Console.WriteLine("BINDDDDDDDDDDDD");
        }
        public void Bind<T>(T value, int value2)
        {
            Console.WriteLine(value);
        }
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