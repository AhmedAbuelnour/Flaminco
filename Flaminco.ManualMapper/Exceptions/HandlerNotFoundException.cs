namespace Flaminco.ManualMapper.Exceptions;

internal sealed class HandlerNotFoundException<TSource, TDestination> : Exception
{
    public HandlerNotFoundException() : base($"Mapper From {typeof(TSource).Name} To {typeof(TDestination).Name} Not Found")
    {
    }
}