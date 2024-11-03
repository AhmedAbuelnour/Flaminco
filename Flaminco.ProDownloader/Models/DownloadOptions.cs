namespace Flaminco.ProDownloader.Models;

public class DownloadOptions
{
    public required string Url { get; set; }
    public required string DownloadPath { get; set; }
    public string? SuggestedFileName { get; set; }
    public required Action<DownloadFileInfo> ProgressCallback { get; set; }
    public int ChunkNumbers { get; set; } = 16;
}