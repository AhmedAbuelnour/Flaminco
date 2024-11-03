using System.Collections.Immutable;
using Flaminco.ProDownloader.Models;
using Flaminco.ProDownloader.Utilities;

namespace Flaminco.ProDownloader.HttpClients;

internal sealed class FileProfileClient
{
    private readonly HttpClient _httpClient;
    private readonly ResumableClient _resumableClient;

    public FileProfileClient(IHttpClientFactory _httpClientFactory)
    {
        _httpClient = _httpClientFactory.CreateClient();
        _resumableClient = new ResumableClient(_httpClientFactory);
    }

    public async Task<FileProfile> GetFileProfileAsync(DownloadOptions downloadOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(downloadOptions.Url);

        if (!(downloadOptions.Url.StartsWith("https://") || downloadOptions.Url.StartsWith("http://")))
            throw new SupportHTTPProtocolOnlyException();

        ArgumentException.ThrowIfNullOrEmpty(downloadOptions.DownloadPath);

        var httpResponseMessage = await _httpClient.GetAsync(downloadOptions.Url,
            HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        var isResumable = await _resumableClient.IsResumableAsync(downloadOptions.Url, cancellationToken);

        var fileSize = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault();

        return new FileProfile
        {
            Name = downloadOptions.SuggestedFileName ??
                   httpResponseMessage.Content.Headers?.ContentDisposition?.FileName ??
                   httpResponseMessage.RequestMessage?.RequestUri?.Segments.LastOrDefault(),
            MediaType = httpResponseMessage.Content.Headers?.ContentType?.MediaType,
            Size = fileSize,
            Extension = httpResponseMessage.Content.Headers.ContentType?.MediaType?.GetFileExtension(),
            IsResumable = isResumable,
            DownloadPath = downloadOptions.DownloadPath,
            Url = downloadOptions.Url,
            ChunksNumber = downloadOptions.ChunkNumbers,
            SegmentMetadata = isResumable && downloadOptions.ChunkNumbers > 1
                ? DownloadHelper.SegmentPosition(fileSize, downloadOptions.ChunkNumbers).Select(segment =>
                    new SegmentMetadata
                    {
                        Url = downloadOptions.Url,
                        Start = segment.Start,
                        End = segment.End,
                        TempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName())
                    }).ToImmutableArray()
                : new SegmentMetadata[]
                {
                    new()
                    {
                        Url = downloadOptions.Url,
                        Start = 0,
                        End = fileSize,
                        TempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName())
                    }
                }
        };
    }
}