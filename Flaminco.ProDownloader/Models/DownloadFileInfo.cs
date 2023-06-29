namespace Flaminco.ProDownloader.Models;

/// <summary>
/// Represents an instance info for the current downloading file.
/// </summary>
public sealed class DownloadFileInfo
{
    /// <summary>
    /// Current Percentage of the current ongoing downloading file
    /// </summary>
    public float CurrentPercentage { get; set; }
    /// <summary>
    /// Current downloading speed of the current ongoing file
    /// </summary>
    public string? DownloadSpeed { get; set; }
    /// <summary>
    /// Current Progress of how much has been downloaded of the ongoing file.
    /// </summary>
    public string? DownloadedProgress { get; set; }
}
