namespace Flaminco.MinimalEndpoints.Options
{
    public interface IExceptionHandlerOptions<TException> where TException : Exception
    {
        string Type { get; set; }
        string Title { get; set; }
        string UserIdentifierTokenName { get; set; }
    }
}
