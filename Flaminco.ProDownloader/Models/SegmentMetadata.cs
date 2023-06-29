﻿namespace Flaminco.ProDownloader.Models;


/// <summary>
/// A class which represents the segment to download
/// </summary>
public sealed class SegmentMetadata
{
    /// <summary>
    ///  Remote Server File location
    /// </summary>
    public required string Url { get; set; }
    /// <summary>
    /// Temp file location
    /// </summary>
    public required string TempPath { get; set; }
    /// <summary>
    /// Segment's Start position
    /// </summary>
    public required long Start { get; set; }

    public long Size { get => End - Start; }
    /// <summary>
    /// Segment's End position
    /// </summary>
    public required long End { get; set; }
}
