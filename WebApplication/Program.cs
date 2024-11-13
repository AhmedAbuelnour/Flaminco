using ErrorOr;
using Flaminco.Validation.Abstractions;
using Flaminco.Validation.Exceptions;
using Flaminco.Validation.Extensions;
using WebApplication1.Validations;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidation<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/weatherforecast", async (IValidation validation) =>
{
    ErrorOr<Success> validationResult = await validation.Validate<Person>(new Person
    {
        Age = 1,
        Name = "AM"
    });

    if (validationResult.IsError)
    {
        throw new ValidationException
        {
            Errors = validationResult.Errors
        };
    }

})
.WithName("GetWeatherForecast");

app.UseExceptionHandler();

app.Run();

