# Flaminco.RediPolly

The `Flaminco.RediPolly` library is a powerful tool designed to simplify the implementation of Redis Pub/Sub features with built-in support for Polly retry policies. Redis Pub/Sub allows for high-performance messaging between components of a distributed system, and Polly provides robust transient-fault-handling mechanisms. This library combines the two to offer a seamless experience in managing Redis channels with resilience.

## Getting Started

### Installation

To install the `Flaminco.RediPolly` package, use the following command in the .NET CLI:

```shell
dotnet add package Flaminco.RediPolly
```

### Setup

Add the `Flaminco.RediPolly` package to your project and configure it to work with your Redis connection.

```csharp

//TPublisherScanner is where your publishers will be located.
services.AddRediPolly<TPublisherScanner>("Your Redis Connection String");

// Here to add your listeners
builder.Services.AddHostedService<ChannelListenerExample>();

```

### Usage

Implement listeners and publishers using provided base classes

```csharp

    // Example of implementing a listener

    public class ChannelListenerExample(IOptions<RedisChannelConfiguration> options) : ChannelListener(options)
    {
        protected override RedisChannel Channel { get => RedisChannel.Literal("Your Redis Channel Name"); }

        // Returning true means don't call the retry policy as i'm managing the call even if it is actually failed. 
        // Returning false means run the retry policy even if it is actually worked.
        protected override ValueTask<bool> Callback(RedisChannel channel, RedisValue value, CancellationToken cancellationToken)
        {
            try
            {
                // Your Logic goes here
                return ValueTask.FromResult(true); // turn off the retry
            }
            catch
            {
                return ValueTask.FromResult(false); // turn on the retry policy
            }
        }

        // the default implementation will retry the callback 5 times exponentially each 3 seconds, and has no fallback behavior.
        // You can override this method to implement your own resilience logic.
        protected override ResiliencePipeline<bool> GetCallbackResiliencePipeline()
        {
            return base.GetCallbackResiliencePipeline();
        }

    }
```

```csharp
    
    // Example of implementing a publisher

    // This should be your publisher (not much to be written here)
    public class PublishAnyMessage(IOptions<RedisChannelConfiguration> options) : ChannelPublisher(options)
    {
        protected override RedisChannel Channel => RedisChannel.Literal("Your Redis Channel Name");
    }

    // this is your publisher locator which it can get your publishers from DI by the publisher type.

    public class Example(IPublisherLocator _locator)
    {
        public async Task Publish()
        {
            if (_locator.GetPublisher<PublishAnyMessage>() is PublishAnyMessage redisPublisher)
            {
                await redisPublisher.PublishAsync(new Counter
                {
                    Count = 5
                });
            }
        }
    }   
```


## Contribution
Contributions are welcome! If you have suggestions, bug reports, or contributions, please submit them as issues or pull requests on our GitHub repository.

## License
This project is licensed under the MIT License.