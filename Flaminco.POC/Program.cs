using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddRabbitMq(builder.Configuration, topology =>
{
    topology.Exchange("orders.exchange", exchange => exchange.Topic()
                                                    .WithDeadLetterExchange()  // Creates "orders.dlx" automatically
                                                    .Queue("poc.order.created.queue", "order.created.#", queue => queue.DeadLetter()));  // Creates "orders.created.dlq" automatically

    //    *(star)Matches exactly one word order.*.new matches order.created.new but not order.created.new.customer
    //    # (hash)	Matches zero or more words	order.created.# matches order.created, order.created.new, order.created.new.customer, etc.
    //    . (dot) Word separator  Separates words in the routing key


});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Map("/publisher", async (IMessagePublisher publisher) =>
{
    await publisher.PublishAsync("orders.exchange", "order.created", new PublisMessageTest
    {
        Id = 1
    });
});

app.Run();

class PublisMessageTest
{
    public int Id { get; set; }
}

class ConsumerMessageTest
{
    public int Id { get; set; }
}


[Queue("poc.order.created.queue")]
class PublishMessageConsumer : MessageConsumer<ConsumerMessageTest>
{
    public override async Task ConsumeAsync(ConsumeContext<ConsumerMessageTest> context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(context.Message.Id);
    }
}