using Flaminco.AzureBus.AMQP.Abstractions;
using Flaminco.AzureBus.AMQP.Attributes;
using Flaminco.AzureBus.AMQP.Extensions;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAMQPClient<Program>(options =>
{
    options.Host = "Endpoint=sb://sb-dev-app.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=onouja6KzYf5AwJWQ/GASEPhEKBIo3lqw+ASbKKsNuc=";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();

public static class Consts
{

    public class Queues
    {
        public const string XX = "notifier-send-class-notification";
    }

}

public class MessageBox : IMessage
{
    public string NotifierId { get; set; }
    public int CourseId { get; set; }
    public int NotificationTypeId { get; set; }
    public string? Content { get; set; }
    public IEnumerable<string>? NotifiedIds { get; set; }
    public string? Metadata { get; set; }
}


[QueueConsumer(queue: "HelloTest")]
public class HelloConsumer2() : MessageConsumer<MessageBox>
{
    public override Task Consume(ConsumeContext<MessageBox> context)
    {

        return Task.CompletedTask;
    }
    public override Task Consume(ConsumeContext<Fault<MessageBox>> context)
    {
        return base.Consume(context);
    }
}

