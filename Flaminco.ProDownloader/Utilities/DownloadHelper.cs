﻿using System.Security.Cryptography;
using System.Text;

namespace Flaminco.ProDownloader.Utilities;

internal static class DownloadHelper
{
    public static IEnumerable<(long Start, long End)> SegmentPosition(long ContentLength, int ChunksNumber)
    {
        var PartSize = (long)Math.Ceiling(ContentLength / (double)ChunksNumber);
        for (var i = 0; i < ChunksNumber; i++)
            yield return (i * PartSize + Math.Min(1, i), Math.Min((i + 1) * PartSize, ContentLength));
    }

    public static string GenerateKeyFromString(string input)
    {
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(input))).ToString();
    }
}