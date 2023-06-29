namespace Flaminco.ProDownloader.Utilities;

internal static class FileSegmenter
{
    internal static IEnumerable<(long Start, long End)> SegmentPosition(long ContentLength, int ChunksNumber)
    {
        long PartSize = (long)Math.Ceiling(ContentLength / (double)ChunksNumber);
        for (var i = 0; i < ChunksNumber; i++)
            yield return (i * PartSize + Math.Min(1, i), Math.Min((i + 1) * PartSize, ContentLength));
        yield break;
    }
}
