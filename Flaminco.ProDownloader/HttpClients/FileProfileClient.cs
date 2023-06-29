using Flaminco.ProDownloader.Models;
using Flaminco.ProDownloader.Utilities;
using System.Collections.Immutable;

namespace Flaminco.ProDownloader.HttpClients;

internal sealed class FileProfileClient
{
    private readonly HttpClient _httpClient;
    private readonly ResumableClient _resumableClient;
    internal FileProfileClient(HttpClient httpClient, ResumableClient resumableClient)
    {
        _httpClient = httpClient;
        _resumableClient = resumableClient;
    }

    internal async Task<FileProfile> GetFileProfileAsync(string url, string downloadPath, int chunkNumbers = 16, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        if (!(url.StartsWith("https://") || url.StartsWith("http://"))) throw new Exception("Only Support Http, Https protocols");

        ArgumentException.ThrowIfNullOrEmpty(downloadPath);

        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        bool isResumable = await _resumableClient.IsResumableAsync(url, cancellationToken);

        long fileSize = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault();

        return new FileProfile
        {
            Name = httpResponseMessage.Content.Headers?.ContentDisposition?.FileName ?? httpResponseMessage.RequestMessage?.RequestUri?.Segments.LastOrDefault(),
            MediaType = httpResponseMessage.Content.Headers?.ContentType?.MediaType,
            Size = fileSize,
            Extension = httpResponseMessage.Content.Headers.ContentType?.MediaType?.GetFileExtension(),
            IsResumable = isResumable,
            DownloadPath = downloadPath,
            Url = url,
            ChunksNumber = chunkNumbers,
            SegmentMetadata = isResumable && chunkNumbers > 1 ? DownloadHelper.SegmentPosition(fileSize, chunkNumbers).Select(segment => new SegmentMetadata
            {
                Url = url,
                Start = segment.Start,
                End = segment.End,
                TempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName())
            }).ToImmutableArray() : new SegmentMetadata[]
            {
                new SegmentMetadata
                {
                    Url = url,
                    Start = 0,
                    End = fileSize,
                    TempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName())
                }
            }
        };
    }
}
