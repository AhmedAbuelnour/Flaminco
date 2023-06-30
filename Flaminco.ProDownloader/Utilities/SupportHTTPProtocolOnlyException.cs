namespace Flaminco.ProDownloader.Utilities;

public class SupportHTTPProtocolOnlyException : Exception
{
    public SupportHTTPProtocolOnlyException() : base("Only Support Http, Https protocols")
    {
    }
}
