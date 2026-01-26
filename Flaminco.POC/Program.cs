using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddRabbitMq(builder.Configuration, topology =>
{
    topology.Exchange("orders", exchange => exchange.Topic()
                                                    .WithDeadLetterExchange()  // Creates "orders.dlx" automatically
                                                    .Queue("orders.created.queue", "orders.created.#", queue => queue.DeadLetter())  // Creates "orders.created.dlq" automatically
                                                    .Queue("orders.shipped.queue", "orders.shipped.#", queue => queue.MaxLength(10000).DeadLetter(dl => dl.Queue("orders.shipped.failed")))
                                                    .Queue("orders.priority.queue", "orders.priority.#", queue => queue.MaxPriority(10).MessageTtl(86400000)));

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
    await publisher.PublishAsync("orders", "order.created", new PublisMessageTest
    {
        Id = 1
    });
});

app.Run();

class PublisMessageTest
{
    public int Id { get; set; }
}

[Exchange("orders", Type = "topic"), Queue("orders.created.queue"), Binding("orders", RoutingKey = "orders.created.#")]
class PublishMessageConsumer : MessageConsumer<PublisMessageTest>
{
    public override async Task ConsumeAsync(ConsumeContext<PublisMessageTest> context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(context.Message.Id);
    }
}