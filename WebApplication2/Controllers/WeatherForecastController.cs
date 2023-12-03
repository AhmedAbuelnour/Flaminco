using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace WebApplication2.Controllers
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaskedAttribute : Attribute
    {
        public int Start { get; private set; }
        public int Length { get; private set; }
        public char MaskingChar { get; private set; }

        public MaskedAttribute(int start, int length, char maskingChar = '*')
        {
            Start = start;
            Length = length;
            MaskingChar = maskingChar;
        }
    }

    public static class MinimalApiExtensions
    {
        public static RouteHandlerBuilder AddMaskFilter(this RouteHandlerBuilder builder)
        {
            // This will modify the request pipeline for the specific route to include a default query value
            builder.AddEndpointFilter<MaskFilter>();

            return builder;
        }
    }


    public class MaskFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            object? result = await next(context);

            if (result is IValueHttpResult objectResult && objectResult.Value is not null)
            {
                MaskProperties(objectResult.Value);
            }

            return result;
        }

        private static void MaskProperties(object obj)
        {
            IEnumerable<PropertyInfo> properties = obj.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string) && Attribute.IsDefined(p, typeof(MaskedAttribute)));

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<MaskedAttribute>() is MaskedAttribute maskedAttr)
                {
                    string? originalValue = (string?)property.GetValue(obj);
                    string? maskedValue = MaskString(originalValue, maskedAttr.Start, maskedAttr.Length, maskedAttr.MaskingChar);
                    property.SetValue(obj, maskedValue);
                }
            }
        }

        public static string? MaskString(string? input, int start, int length, char maskingChar)
        {
            if (input == null) return null;

            if (start < 0 || length < 0 || start + length > input.Length) return input; // Or throw an exception

            return string.Concat(input.AsSpan(0, start), new string(maskingChar, length), input.AsSpan(start + length));
        }
    }

    public class MaskedAttributeFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
            {
                MaskProperties(objectResult.Value);
            }
            await next();
        }

        private static void MaskProperties(object obj)
        {
            IEnumerable<PropertyInfo> properties = obj.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<MaskedAttribute>() is MaskedAttribute maskedAttr)
                {
                    string? originalValue = (string?)property.GetValue(obj);
                    string? maskedValue = MaskString(originalValue, maskedAttr.Start, maskedAttr.Length, maskedAttr.MaskingChar);
                    property.SetValue(obj, maskedValue);
                }
            }
        }

        public static string? MaskString(string? input, int start, int length, char maskingChar)
        {
            if (input == null) return null;

            if (start < 0 || length < 0 || start + length > input.Length) return input; // Or throw an exception

            return string.Concat(input.AsSpan(0, start), new string(maskingChar, length), input.AsSpan(start + length));
        }
    }


    public class MyModel
    {
        [Masked(1, 3)]
        public string SensitiveData { get; set; }
    }


    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            return Ok(new MyModel
            {
                SensitiveData = "123456789"
            });
        }
    }
}
