using System.Net;
using System.Net.Http.Headers;

namespace Flaminco.ProDownloader.HttpClients;

public class ResumableClient
{
    private readonly HttpClient _httpClient;
    public ResumableClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(1, 1);
    }
    public async Task<bool> IsResumableAsync(string Url, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage Result = await _httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        return Result.StatusCode == HttpStatusCode.PartialContent;
    }
}
