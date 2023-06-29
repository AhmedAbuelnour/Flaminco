namespace Flaminco.ProDownloader.Models;

public sealed class FileProfile
{
    public required string Url { get; set; }
    public required string? Name { get; set; } = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
    public required string? Extension { get; set; }
    public required string? MediaType { get; set; }
    public required long Size { get; set; }
    public required string DownloadPath { get; set; }
    public bool IsResumable { get; set; }
    public int ChunksNumber { get; set; }
    public required IReadOnlyCollection<SegmentMetadata> SegmentMetadata { get; set; }
}
