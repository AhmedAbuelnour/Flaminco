using Flaminco.ProDownloader.Utilities;
using System.Net.Http.Headers;

namespace Flaminco.ProDownloader.HttpClients
{
    public class SegementProfileClient
    {

        private readonly IHttpClientFactory _httpClientFactory;
        public SegementProfileClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ServerSegmentProfile?> GetServerSegmentProfileAsync(string url, long start, long end, CancellationToken cancellationToken = default)
        {
            HttpClient _httpClient = _httpClientFactory.CreateClient();

            _httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(start, end);

            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                return default;
            }

            return new ServerSegmentProfile
            {
                Size = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault(),
                Stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken),
                TotalReadBytes = 0,
                Start = start,
                End = end,
                FileLocation = Path.GetTempFileName()
            };
        }
    }
}
