using System.Diagnostics;

namespace Flaminco.ProDownloader.Utilities;

public class ProfileDownloader
{
    private readonly PipeController _pipelineController;
    private readonly Stopwatch SWatch;

    public ProfileDownloader()
    {
        SWatch = new Stopwatch();
        _pipelineController = new PipeController();
    }

    public async Task DownloadAsync(ServerFileProfile profile, Action<DownloadInfo> CurrentProgress, CancellationToken cancellationToken = default)
    {
        SWatch.Start();

        Task writing = _pipelineController.WriteInPipeAsync(profile, () => CurrentProgress.Invoke(ProgressCallback(profile, SWatch)), cancellationToken);

        Task reading = _pipelineController.ReadFromPipeAsync(profile, cancellationToken);

        await Task.WhenAll(reading, writing);

        SWatch.Stop();

        SWatch.Reset();
    }

    public async Task DownloadAsync(ServerSegmentProfile profile, Action<DownloadInfo> CurrentProgress, CancellationToken cancellationToken = default)
    {
        SWatch.Start();

        Task writing = _pipelineController.WriteInPipeAsync(profile, () => CurrentProgress.Invoke(ProgressCallback(profile, SWatch)), cancellationToken);

        Task reading = _pipelineController.ReadFromPipeAsync(profile, cancellationToken);

        await Task.WhenAll(reading, writing);

        SWatch.Stop();

        SWatch.Reset();
    }

    public DownloadInfo ProgressCallback(BaseProfile profile, Stopwatch SWatch)
       => new DownloadInfo
       {
           CurrentPercentage = ((profile.TotalReadBytes) / (float)profile.Size) * 100, // Gets the Current Percentage
           DownloadSpeed = Convert.ToInt64((profile.TotalReadBytes / SWatch.Elapsed.TotalSeconds)).SizeSuffix(), // Get The Current Speed
           DownloadedProgress = string.Format("{0} MB's / {1} MB's", (profile.TotalReadBytes / 1024d / 1024d).ToString("0.00"), (profile.Size / 1024d / 1024d).ToString("0.00")) // Get How much has been downloaded
       };
}
