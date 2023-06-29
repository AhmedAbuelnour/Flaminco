using Flaminco.ProDownloader.Models;
using Flaminco.ProDownloader.Utilities;
using System.Diagnostics;
using System.IO.Pipelines;

namespace Flaminco.ProDownloader.HttpClients;

public sealed class DownloaderClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Stopwatch _stopwatch;
    private long _totalReadBytes = 0;
    public DownloaderClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _stopwatch = new Stopwatch();
    }

    public async Task DownloadAsync(string url, string downloadPath, Action<DownloadFileInfo> CurrentProgress, int chunkNumbers = 16, CancellationToken cancellationToken = default)
    {
        FileProfileClient fileProfileClient = new(_httpClientFactory.CreateClient(), new ResumableClient(_httpClientFactory.CreateClient()));

        FileProfile fileProfile = await fileProfileClient.GetFileProfileAsync(url, downloadPath, chunkNumbers, cancellationToken);

        await DownloadAsync(fileProfile, CurrentProgress, cancellationToken);

        await ReconstructProfilesAsync(fileProfile, cancellationToken);
    }


    private async Task DownloadAsync(FileProfile profile, Action<DownloadFileInfo> CurrentProgress, CancellationToken cancellationToken = default)
    {
        _stopwatch.Start();

        await Parallel.ForEachAsync(profile.SegmentMetadata, cancellationToken, async (segment, token) =>
        {
            SegmentClient segmentClient = new(_httpClientFactory.CreateClient(), new Pipe());

            await segmentClient.DownloadAsync(segment, (totalReadBytes) =>
            {
                _totalReadBytes += totalReadBytes;

                CurrentProgress(ProgressCallback(profile));

            }, token);

        });

        _stopwatch.Stop();

        _stopwatch.Reset();

        _totalReadBytes = 0;
    }

    private async Task ReconstructProfilesAsync(FileProfile profile, CancellationToken cancellationToken = default)
    {
        string FilePath = Path.Combine(profile.DownloadPath, profile.Name);

        using Stream localFileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);

        foreach (var Segment in profile.SegmentMetadata.OrderBy(x => x.Start))
        {
            localFileStream.Seek(Segment.Start, SeekOrigin.Begin);

            using (Stream tempStream = new FileStream(Segment.TempPath, FileMode.Open, FileAccess.Read))
            {
                await tempStream.CopyToAsync(localFileStream, cancellationToken);
            }

            File.Delete(Segment.TempPath);
        }

        await Task.CompletedTask;
    }

    private DownloadFileInfo ProgressCallback(FileProfile profile)
    {
        return new()
        {
            CurrentPercentage = (_totalReadBytes / (float)profile.Size) * 100, // Gets the Current Percentage
            DownloadSpeed = Convert.ToInt64(_totalReadBytes / _stopwatch.Elapsed.TotalSeconds).SizeSuffix(), // Get The Current Speed
            DownloadedProgress = string.Format("{0} MB's / {1} MB's", (_totalReadBytes / 1024d / 1024d).ToString("0.00"), (profile.Size / 1024d / 1024d).ToString("0.00")) // Get How much has been downloaded
        };
    }
}
