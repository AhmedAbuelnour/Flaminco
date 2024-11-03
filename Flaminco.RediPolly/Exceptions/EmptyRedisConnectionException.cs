namespace Flaminco.RediPolly.Exceptions;

internal sealed class EmptyRedisConnectionException(string argumentName)
    : ArgumentException("You need provider a correct redis connection string", argumentName)
{
}