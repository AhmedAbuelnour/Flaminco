using Flaminco.ProDownloader.Models;
using Flaminco.ProDownloader.Utilities;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.Json;

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

    public async Task DownloadAsync(DownloadOptions downloadOptions, CancellationToken cancellationToken = default)
    {
        FileProfileClient fileProfileClient = new(_httpClientFactory);

        FileProfile? fileProfile = await GetProfileConfiguration(downloadOptions.Url, cancellationToken);

        if (fileProfile is null)
        {
            fileProfile = await fileProfileClient.GetFileProfileAsync(downloadOptions, cancellationToken);

            await SaveProfileConfiguration(fileProfile, cancellationToken);
        }

        await DownloadAsync(fileProfile!, downloadOptions.ProgressCallback, cancellationToken);

        await ReconstructSegmentsAsync(fileProfile!, cancellationToken);
    }

    private async Task<FileProfile?> GetProfileConfiguration(string url, CancellationToken cancellationToken = default)
    {
        string configuration = Path.Combine(Path.GetTempPath(), $"{DownloadHelper.GenerateKeyFromString(url)}.txt");

        if (File.Exists(configuration))
        {
            string content = await File.ReadAllTextAsync(configuration, cancellationToken);

            return JsonSerializer.Deserialize<FileProfile>(content);
        }
        else
        {
            return default;
        }
    }

    private Task SaveProfileConfiguration(FileProfile fileProfile, CancellationToken cancellationToken = default)
    {
        string configuration = Path.Combine(Path.GetTempPath(), $"{DownloadHelper.GenerateKeyFromString(fileProfile.Url)}.txt");

        if (File.Exists(configuration))
        {
            File.Delete(configuration);
        }

        string content = JsonSerializer.Serialize(fileProfile);

        return File.WriteAllTextAsync(configuration, content, cancellationToken);
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
                Console.WriteLine($"Total Read Bytes: ${_totalReadBytes}");
                CurrentProgress(ProgressCallback(profile));
            }, token);
        });

        _stopwatch.Stop();

        _stopwatch.Reset();

        _totalReadBytes = 0;
    }

    private async Task ReconstructSegmentsAsync(FileProfile profile, CancellationToken cancellationToken = default)
    {
        string FilePath = Path.Combine(profile.DownloadPath, profile.Name);

        using Stream localFileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);

        foreach (var Segment in profile.SegmentMetadata.OrderBy(a => a.Start))
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
