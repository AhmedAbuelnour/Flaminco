using Flaminco.ProDownloader.Utilities;

namespace Flaminco.ProDownloader.HttpClients
{
    public class FileProfileClient
    {
        private readonly HttpClient _httpClient;
        public FileProfileClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ServerFileProfile?> GetServerFileProfileAsync(string url, bool isResumable, string filePath, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                return default;
            }

            return new ServerFileProfile
            {
                Name = httpResponseMessage.Content.Headers?.ContentDisposition?.FileName ?? httpResponseMessage.RequestMessage?.RequestUri?.Segments.LastOrDefault(),
                MediaType = httpResponseMessage.Content.Headers?.ContentType?.MediaType,
                Size = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault(),
                Extension = httpResponseMessage.Content.Headers.ContentType?.MediaType?.GetFileExtension(),
                IsResumable = isResumable,
                Stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken),
                TotalReadBytes = 0,
                FileLocation = filePath
            };
        }


    }
}
