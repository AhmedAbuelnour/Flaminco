namespace Flaminco.ProDownloader.Utilities;

using System.IO;


public class BaseProfile
{
    /// <summary>
    /// Segment's Partial Stream
    /// </summary>
    public Stream Stream { get; set; }
    /// <summary>
    /// Segment's Size
    /// </summary>
    public long Size { get; set; }
    public string FileLocation { get; set; }
    /// <summary>
    /// Total bytes that have been read until now
    /// </summary>
    public long TotalReadBytes { get; set; }
}

/// <summary>
/// A class which represents the segment to download
/// </summary>
public class ServerSegmentProfile : BaseProfile
{
    /// <summary>
    /// Segment's Start position
    /// </summary>
    public long Start { get; set; }
    /// <summary>
    /// Segment's End position
    /// </summary>
    public long End { get; set; }
    /// <summary>
    /// Temp file location
    /// </summary>
}

public class ServerFileProfile : BaseProfile
{
    public string? Name { get; set; }
    public string? Extension { get; set; }
    public string? MediaType { get; set; }
    public bool IsResumable { get; set; }
}


/// <summary>
/// Represents an instance info for the current downloading file.
/// </summary>
public class DownloadInfo
{
    /// <summary>
    /// Current Percentage of the current ongoing downloading file
    /// </summary>
    public float CurrentPercentage { get; set; }
    /// <summary>
    /// Current downloading speed of the current ongoing file
    /// </summary>
    public string DownloadSpeed { get; set; }
    /// <summary>
    /// Current Progress of how much has been downloaded of the ongoing file.
    /// </summary>
    public string DownloadedProgress { get; set; }
}
