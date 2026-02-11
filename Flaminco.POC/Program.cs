using Flaminco.MinimalEndpoints.Abstractions;
using Flaminco.MinimalEndpoints.Extensions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//builder.Services.AddRabbitMq(builder.Configuration, topology =>
//{
//    // Topic exchange for order events
//    topology.Exchange("orders.exchange", exchange => exchange.Topic()
//                                                    .WithDeadLetterExchange()  // Creates "orders.dlx" automatically
//                                                    .Queue("poc.order.created.queue", "order.created.#", queue => queue.DeadLetter()));  // Creates "orders.created.dlq" automatically

//    // Direct exchange for RPC
//    topology.Exchange("rpc.exchange", exchange => exchange.Direct()
//                                                    .Queue("calculator.queue", "calculator"));

//    //    *(star)Matches exactly one word order.*.new matches order.created.new but not order.created.new.customer
//    //    # (hash)	Matches zero or more words	order.created.# matches order.created, order.created.new, order.created.new.customer, etc.
//    //    . (dot) Word separator  Separates words in the routing key
//});

// Add RPC client support
//builder.Services.AddRabbitMqRpc();


builder.Services.AddValidators<Program>();

builder.Services.AddRedisEventBus(options =>
{
    // Important: BLPOP can block up to PopTimeoutSeconds, so Redis client timeout must be higher.
    // Otherwise you'll frequently see RedisTimeoutException when the queue is empty.
    var redisConfiguration = new ConfigurationOptions
    {
        EndPoints = { "localhost:6379" },
        AbortOnConnectFail = false,
        ConnectTimeout = 15000,
        SyncTimeout = 15000,
        AsyncTimeout = 15000
    };

    options.ConnectionString = "localhost:6379";
    options.ConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration); // Reuse the connection multiplexer for better performance
    options.QueueKey = "poc.redis.events"; // Redis list key for the event queue
    options.PopTimeoutSeconds = 3; // Keep below Redis client timeout to avoid false timeout exceptions on idle queues
}, typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/Get", async (IEventBus eventBus) =>
{
    await eventBus.Publish(new OrderCreated
    {
        OrderId = 1,
        CreatedAt = DateTime.UtcNow
    }, CancellationToken.None);

});


//// Publisher endpoint for regular messages
//app.MapGet("/publisher", async (IMessagePublisher publisher, IAttachmentGetAllOperation attachmentOperation) =>
//{
//    await publisher.PublishAsync("orders.exchange", "order.created", new PublisMessageTest
//    {
//        Id = 1
//    });

//    return Results.Ok("Message published successfully");
//});


//// RPC client endpoint
//app.MapGet("/calculate", async (IMessageRpcClient rpcClient, int a = 10, int b = 5, string operation = "add") =>
//{
//    try
//    {
//        var request = new CalculateRequest
//        {
//            A = a,
//            B = b,
//            Operation = operation
//        };

//        var response = await rpcClient.CallAsync<CalculateRequest, CalculateResponse>(
//            "rpc.exchange",
//            "calculator",
//            request,
//            timeoutMs: 5000);

//        return Results.Ok(new
//        {
//            request = request,
//            response = response
//        });
//    }
//    catch (TimeoutException ex)
//    {
//        return Results.Problem($"RPC call timed out: {ex.Message}", statusCode: 504);
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"RPC call failed: {ex.Message}", statusCode: 500);
//    }
//});

app.Run();


public class OrderCreated : IDomainEvent
{
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderCreatedHandler : IDomainEventHandler<OrderCreated>
{
    private readonly ILogger<OrderCreatedHandler> _logger;
    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(OrderCreated domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling OrderCreated event for OrderId: {OrderId} at {CreatedAt}", domainEvent.OrderId, domainEvent.CreatedAt);
        return ValueTask.CompletedTask;
    }
}

// DTO classes
class PublisMessageTest
{
    public int Id { get; set; }
}

class ConsumerMessageTest
{
    public int Id { get; set; }
}

class CalculateRequest
{
    public int A { get; set; }
    public int B { get; set; }
    public string Operation { get; set; } = string.Empty;
}

class CalculateResponse
{
    public int Result { get; set; }
    public string Message { get; set; } = string.Empty;
}

//// Regular message consumer
//[Queue("poc.order.created.queue")]
//class PublishMessageConsumer : MessageConsumer<ConsumerMessageTest>
//{
//    private readonly ILogger<PublishMessageConsumer> _logger;

//    public PublishMessageConsumer(ILogger<PublishMessageConsumer> logger)
//    {
//        _logger = logger;
//    }

//    public override async Task ConsumeAsync(ConsumeContext<ConsumerMessageTest> context, CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Received order created message with Id: {Id}", context.Message.Id);
//        await Task.CompletedTask;
//    }
//}

//// RPC server consumer
//[Queue("calculator.queue")]
//class CalculatorRpcConsumer : RpcMessageConsumer<CalculateRequest, CalculateResponse>
//{
//    private readonly ILogger<CalculatorRpcConsumer> _logger;

//    public CalculatorRpcConsumer(ILogger<CalculatorRpcConsumer> logger)
//    {
//        _logger = logger;
//    }

//    public override async Task<CalculateResponse> HandleAsync(
//        RpcContext<CalculateRequest> context,
//        CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Received RPC request: {A} {Operation} {B}",
//            context.Message.A, context.Message.Operation, context.Message.B);

//        var result = context.Message.Operation.ToLower() switch
//        {
//            "add" => context.Message.A + context.Message.B,
//            "subtract" => context.Message.A - context.Message.B,
//            "multiply" => context.Message.A * context.Message.B,
//            "divide" => context.Message.B != 0 ? context.Message.A / context.Message.B : throw new DivideByZeroException(),
//            _ => throw new ArgumentException($"Invalid operation: {context.Message.Operation}")
//        };

//        _logger.LogInformation("Calculated result: {Result}", result);

//        await Task.Delay(100, cancellationToken); // Simulate some processing

//        return new CalculateResponse
//        {
//            Result = result,
//            Message = $"{context.Message.A} {context.Message.Operation} {context.Message.B} = {result}"
//        };
//    }

//    public override async Task<ErrorHandlingResult> OnErrorAsync(
//        RpcContext<CalculateRequest> context,
//        Exception exception,
//        CancellationToken cancellationToken = default)
//    {
//        _logger.LogError(exception, "Error processing RPC request");

//        // For RPC, we typically want to reject failed requests
//        // so the caller gets a timeout instead of waiting indefinitely
//        return await Task.FromResult(ErrorHandlingResult.Reject);
//    }
//}
