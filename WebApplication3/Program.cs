using Flaminco.TickCronos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//builder.Services.AddAMQPClient<Program>(options =>
//{
//    options.SkipMessageTypeMatching = true;
//    options.Host =
//        "Endpoint=sb://sb-dev-app.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=onouja6KzYf5AwJWQ/GASEPhEKBIo3lqw+ASbKKsNuc=";
//});

// Define the time zone you want



builder.Services.AddTickCronosJob<MyTickCronosExample2>(a =>
{
    a.CronExpression = "0 */1 * * * *";
    a.TimeProvider = new TimeZoneTimeProvider(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
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

public class MyTickCronosExample1(ITickCronosConfig<MyTickCronosExample1> config, IServiceProvider serviceProvider, ILogger<MyTickCronosExample1> logger)
    : TickCronosJobService(config.CronExpression, config.TimeProvider, serviceProvider, logger)
{
    public override string CronJobName => nameof(MyTickCronosExample1);

    public override async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Your job logic here
        Console.WriteLine("Running scheduled job...");
        await Task.Delay(1000, cancellationToken);  // Simulated work
    }
}

public class MyTickCronosExample2(ITickCronosConfig<MyTickCronosExample2> config, IServiceProvider serviceProvider, ILogger<MyTickCronosExample2> logger)
    : TickCronosJobService(config.CronExpression, config.TimeProvider, serviceProvider, logger)
{
    public override string CronJobName => nameof(MyTickCronosExample2);

    public override async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Your job logic here
        Console.WriteLine("Running scheduled job...");
        await Task.Delay(1000, cancellationToken);  // Simulated work
    }
}


public class TimeZoneTimeProvider(TimeZoneInfo timeZoneInfo) : TimeProvider
{
    private readonly TimeZoneInfo _timeZoneInfo = timeZoneInfo ?? throw new ArgumentNullException(nameof(timeZoneInfo));

    public override DateTimeOffset GetUtcNow()
    {
        // Return the current time in the specified time zone, converted to UTC
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZoneInfo);
    }

    public override TimeZoneInfo LocalTimeZone => _timeZoneInfo;
}