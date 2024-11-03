using System.Text.Json.Serialization;

namespace Flaminco.ProDownloader.Models;

/// <summary>
///     A class which represents the segment to download
/// </summary>
internal sealed class SegmentMetadata
{
    /// <summary>
    ///     Remote Server File location
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    ///     Temp file location
    /// </summary>
    public required string TempPath { get; set; }

    /// <summary>
    ///     Segment's Start position
    /// </summary>
    public required long Start { get; set; }

    public long Size => End - Start;

    /// <summary>
    ///     Segment's End position
    /// </summary>
    public required long End { get; set; }

    /// <summary>
    ///     Indicate the total bytes that has been downloaded.
    /// </summary>
    [JsonIgnore]
    internal long TotalReadBytes { get; set; }
}