namespace Flaminco.MinimalMediatR.Options
{
    public interface IExceptionHandlerOptions<TException> where TException : Exception
    {
        string Type { get; set; }
        string Title { get; set; }
        string UserIdTokenName { get; set; }
    }
}
