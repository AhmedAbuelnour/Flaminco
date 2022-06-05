using System.Net;
using System.Net.Http.Headers;

namespace Flaminco.ProDownloader.Utilities;

public class ResumableHttpClient
{
    private readonly HttpClient _httpClient;
    public ResumableHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<bool> IsResumableAsync(string Url, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(1, 1);
        using HttpResponseMessage Result = await _httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        return Result.StatusCode == HttpStatusCode.PartialContent;
    }
}
