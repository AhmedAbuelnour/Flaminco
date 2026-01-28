using Flaminco.RabbitMQ.AMQP.Abstractions;
using Flaminco.RabbitMQ.AMQP.Attributes;
using Flaminco.RabbitMQ.AMQP.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddRabbitMq(builder.Configuration, topology =>
{
    // Topic exchange for order events
    topology.Exchange("orders.exchange", exchange => exchange.Topic()
                                                    .WithDeadLetterExchange()  // Creates "orders.dlx" automatically
                                                    .Queue("poc.order.created.queue", "order.created.#", queue => queue.DeadLetter()));  // Creates "orders.created.dlq" automatically

    // Direct exchange for RPC
    topology.Exchange("rpc.exchange", exchange => exchange.Direct()
                                                    .Queue("calculator.queue", "calculator"));

    //    *(star)Matches exactly one word order.*.new matches order.created.new but not order.created.new.customer
    //    # (hash)	Matches zero or more words	order.created.# matches order.created, order.created.new, order.created.new.customer, etc.
    //    . (dot) Word separator  Separates words in the routing key
});

// Add RPC client support
builder.Services.AddRabbitMqRpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Publisher endpoint for regular messages
app.MapGet("/publisher", async (IMessagePublisher publisher) =>
{
    await publisher.PublishAsync("orders.exchange", "order.created", new PublisMessageTest
    {
        Id = 1
    });

    return Results.Ok("Message published successfully");
});

// RPC client endpoint
app.MapGet("/calculate", async (IMessageRpcClient rpcClient, int a = 10, int b = 5, string operation = "add") =>
{
    try
    {
        var request = new CalculateRequest
        {
            A = a,
            B = b,
            Operation = operation
        };

        var response = await rpcClient.CallAsync<CalculateRequest, CalculateResponse>(
            "rpc.exchange",
            "calculator",
            request,
            timeoutMs: 5000);

        return Results.Ok(new
        {
            request = request,
            response = response
        });
    }
    catch (TimeoutException ex)
    {
        return Results.Problem($"RPC call timed out: {ex.Message}", statusCode: 504);
    }
    catch (Exception ex)
    {
        return Results.Problem($"RPC call failed: {ex.Message}", statusCode: 500);
    }
});

app.Run();

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

// Regular message consumer
[Queue("poc.order.created.queue")]
class PublishMessageConsumer : MessageConsumer<ConsumerMessageTest>
{
    private readonly ILogger<PublishMessageConsumer> _logger;

    public PublishMessageConsumer(ILogger<PublishMessageConsumer> logger)
    {
        _logger = logger;
    }

    public override async Task ConsumeAsync(ConsumeContext<ConsumerMessageTest> context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received order created message with Id: {Id}", context.Message.Id);
        await Task.CompletedTask;
    }
}

// RPC server consumer
[Queue("calculator.queue")]
class CalculatorRpcConsumer : RpcMessageConsumer<CalculateRequest, CalculateResponse>
{
    private readonly ILogger<CalculatorRpcConsumer> _logger;

    public CalculatorRpcConsumer(ILogger<CalculatorRpcConsumer> logger)
    {
        _logger = logger;
    }

    public override async Task<CalculateResponse> HandleAsync(
        RpcContext<CalculateRequest> context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received RPC request: {A} {Operation} {B}",
            context.Message.A, context.Message.Operation, context.Message.B);

        var result = context.Message.Operation.ToLower() switch
        {
            "add" => context.Message.A + context.Message.B,
            "subtract" => context.Message.A - context.Message.B,
            "multiply" => context.Message.A * context.Message.B,
            "divide" => context.Message.B != 0 ? context.Message.A / context.Message.B : throw new DivideByZeroException(),
            _ => throw new ArgumentException($"Invalid operation: {context.Message.Operation}")
        };

        _logger.LogInformation("Calculated result: {Result}", result);

        await Task.Delay(100, cancellationToken); // Simulate some processing

        return new CalculateResponse
        {
            Result = result,
            Message = $"{context.Message.A} {context.Message.Operation} {context.Message.B} = {result}"
        };
    }

    public override async Task<ErrorHandlingResult> OnErrorAsync(
        RpcContext<CalculateRequest> context,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Error processing RPC request");

        // For RPC, we typically want to reject failed requests
        // so the caller gets a timeout instead of waiting indefinitely
        return await Task.FromResult(ErrorHandlingResult.Reject);
    }
}