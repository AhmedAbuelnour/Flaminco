using Azure.Messaging.ServiceBus;
using Flaminco.AdvancedHybridCache.Extensions;
using Flaminco.AzureBus.AMQP.Abstractions;
using Flaminco.AzureBus.AMQP.Attributes;
using Flaminco.AzureBus.AMQP.Extensions;
using Flaminco.AzureBus.AMQP.Services;
using Flaminco.DualMapper.Extensions;
using Flaminco.ManualMapper.Extensions;
using Flaminco.MinimalMediatR.Extensions;
using Flaminco.StateMachine;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStateMachine<Program>();

builder.Services.AddAuthentication().AddBearerToken(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddManualMapper<Program>();

builder.Services.AddDualMappers<Program>();

builder.Services.AddStackExchangeRedisCache(a =>
{
    a.Configuration = "172.17.7.5:6379";
});

builder.Services.AddAdvancedHybridCache(a =>
{
    a.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1),
        Flags = HybridCacheEntryFlags.DisableLocalCache
    };
});

string ConnectionString = "Endpoint=sb://sb-dev-app.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=onouja6KzYf5AwJWQ/GASEPhEKBIo3lqw+ASbKKsNuc=";

builder.Services.AddAmqpClient(ConnectionString, o =>
{
    o.Identifier = "test";
}, typeof(Program).Assembly);


builder.Services.AddBusinessExceptionHandler(a =>
{
    a.Type = "https://dga.error.codes/index#";
});

builder.Services.AddProblemDetails();


var app = builder.Build();

app.MapGet("/test", async (XMessagePublisher messagePublisher) =>
{
    await messagePublisher.PublishAsync(new ServiceBusMessage
    {
        Body = BinaryData.FromObjectAsJson(new XMessage
        {
            Message = "Hello World"
        }),
    }, CancellationToken.None);

    return Results.Ok();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.UseExceptionHandler();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});


app.Run();


public class XMessage
{
    public string? Message { get; set; }
}

public class XMessagePublisher : MessagePublisher
{
    public XMessagePublisher(AmqpConnectionProvider connectionProvider) : base(connectionProvider)
    {
    }

    protected override string EntityPath => "test_topic_z";

    protected override bool IsTopic => true;
}


[TopicConsumer(topic: "test_topic_z", subscription: "subscription_1")]
public class XMessageConsumer : MessageConsumer<XMessage>
{

    public override Task ConsumeAsync(XMessage message, ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message.Message);

        return Task.CompletedTask;
    }
}

[TopicConsumer(topic: "test_topic_z", subscription: "subscription_2")]
public class XYMessageConsumer : MessageConsumer<XMessage>
{
    public override Task ConsumeAsync(XMessage message, ServiceBusReceivedMessage serviceBusMessage, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(message.Message);

        return Task.CompletedTask;
    }
}