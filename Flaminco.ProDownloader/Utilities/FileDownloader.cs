using System.Diagnostics;

namespace Flaminco.ProDownloader.Utilities;

public class FileDownloader
{
    private readonly PipeController _pipelineController;
    public FileDownloader(PipeController _pipeController)
    {
        _pipelineController = _pipeController;
    }

    public async Task DownloadAsync(ServerFileProfile profile, Action<DownloadInfo> CurrentProgress, CancellationToken cancellationToken = default)
    {
        Stopwatch SWatch = new Stopwatch();

        SWatch.Start();

        Task writing = _pipelineController.WriteInPipeAsync(profile, () => CurrentProgress.Invoke(ProgressCallback(profile, SWatch)), cancellationToken);

        Task reading = _pipelineController.ReadFromPipeAsync(profile, cancellationToken);

        await Task.WhenAll(reading, writing);

        SWatch.Stop();

        SWatch.Reset();
    }

    public async Task DownloadAsync(ServerSegmentProfile profile, Action<DownloadInfo> CurrentProgress, CancellationToken cancellationToken = default)
    {
        Stopwatch SWatch = new Stopwatch();

        SWatch.Start();

        Task writing = _pipelineController.WriteInPipeAsync(profile, () => CurrentProgress.Invoke(ProgressCallback(profile, SWatch)), cancellationToken);

        Task reading = _pipelineController.ReadFromPipeAsync(profile, cancellationToken);

        await Task.WhenAll(reading, writing);

        SWatch.Stop();

        SWatch.Reset();
    }

    private static DownloadInfo ProgressCallback(BaseProfile profile, Stopwatch SWatch)
       => new()
       {
           CurrentPercentage = ((profile.TotalReadBytes) / (float)profile.Size) * 100, // Gets the Current Percentage
           DownloadSpeed = Convert.ToInt64((profile.TotalReadBytes / SWatch.Elapsed.TotalSeconds)).SizeSuffix(), // Get The Current Speed
           DownloadedProgress = string.Format("{0} MB's / {1} MB's", (profile.TotalReadBytes / 1024d / 1024d).ToString("0.00"), (profile.Size / 1024d / 1024d).ToString("0.00")) // Get How much has been downloaded
       };
}
