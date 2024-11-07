using Flaminco.Contracts;
using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Extensions;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddAMQPClient<Program>(options =>
{
    options.Host = "amqp://guest:guest@localhost:5672";
    options.Username = "guest";
    options.Password = "guest";
});


// Define the time zone you want



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();

[QueueConsumer("HelloTest")]
public class ExampleConsumer : MessageConsumer<ExampleRequest>
{
    public override async Task Consume(ConsumeContext<ExampleRequest> context)
    {
        await context.RespondAsync<ExampleResponse>(new ExampleResponse
        {
            Message = "This is a test message"
        });
    }
}