namespace Flaminco.ManualMapper.Exceptions;

public class HandlerNotFoundException<TSource, TDestination> : Exception
{
    public HandlerNotFoundException() : base(
        $"Mapper From {typeof(TSource).Name} To {typeof(TDestination).Name} Not Found")
    {
    }
}