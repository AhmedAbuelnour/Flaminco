using Flaminco.ProDownloader.HttpClients;
using System.Runtime.CompilerServices;

namespace Flaminco.ProDownloader.Utilities;

public class FileProfileCreator
{
    private readonly ResumableClient _resumableClient;

    private readonly FileProfileClient _fileProfileClient;

    private readonly SegementProfileClient _segementProfileClient;
    public string? DirectoryPath { get; private set; }
    public string? FileName { get; private set; }
    public string? Url { get; private set; }
    string FilePath { get => Path.Combine(DirectoryPath!, FileName!); }
    public FileProfileCreator(ResumableClient resumableClient, FileProfileClient fileProfileClient, SegementProfileClient segementProfileClient)
    {
        _resumableClient = resumableClient;
        _fileProfileClient = fileProfileClient;
        _segementProfileClient = segementProfileClient;
    }


    public void Initializer(string url, string directoryPath, string fileName)
    {
        Url = url;
        DirectoryPath = directoryPath;
        FileName = fileName;
    }

    /// <summary>
    /// Loads the downloader with the required information about the remote file.
    /// </summary>
    /// <returns>
    /// returns a profile, which contains all information required to download the file.
    /// </returns>
    public async Task<ServerFileProfile?> CreateFileProfileAsync(string Url, string DirectoryPath, string FileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(Url);

        // Validation for https and http
        if (!(Url.StartsWith("https://") || Url.StartsWith("http://"))) throw new Exception("Only Support Http, Https protocols");

        ArgumentException.ThrowIfNullOrEmpty(DirectoryPath);

        ArgumentException.ThrowIfNullOrEmpty(FileName);

        bool isResumable = await _resumableClient.IsResumableAsync(Url, cancellationToken);

        return await _fileProfileClient.GetServerFileProfileAsync(Url, isResumable, FilePath, cancellationToken);
    }
    /// <summary>
    /// Loads the downloader with the required information about the remote file in segments.
    /// </summary>
    /// <returns>
    /// returns list of profiles, which contains all information required to download the file.
    /// </returns>
    public async IAsyncEnumerable<ServerSegmentProfile?> CreateSegmentProfilesAsync(int SegmentNumbers = 8, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {

        ArgumentException.ThrowIfNullOrEmpty(Url);

        // Validation for https and http
        if (!(Url.StartsWith("https://") || Url.StartsWith("http://"))) throw new Exception("Only Support Http, Https protocols");

        ArgumentException.ThrowIfNullOrEmpty(DirectoryPath);

        ArgumentException.ThrowIfNullOrEmpty(FileName);

        ServerFileProfile? serverFileProfile = await CreateFileProfileAsync(Url, DirectoryPath, FileName, cancellationToken);

        static IEnumerable<(long Start, long End)> SegmentPosition(long ContentLength, int ChunksNumber)
        {
            long PartSize = (long)Math.Ceiling(ContentLength / (double)ChunksNumber);
            for (var i = 0; i < ChunksNumber; i++)
                yield return (i * PartSize + Math.Min(1, i), Math.Min((i + 1) * PartSize, ContentLength));
            yield break;
        }

        if (serverFileProfile?.IsResumable ?? false)
        {
            foreach ((long Start, long End) in SegmentPosition(serverFileProfile.Size, SegmentNumbers))
            {
                yield return await _segementProfileClient.GetServerSegmentProfileAsync(Url, Start, End, cancellationToken);
            }
            yield break;
        }
        else
        {
            throw new Exception("Can't create segments for not resumable remote resource");
        }
    }


    public async Task ReconstructSegmentProfilesAsync(IEnumerable<ServerSegmentProfile> profiles, CancellationToken cancellationToken = default)
    {
        using Stream localFileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);

        foreach (var Segment in profiles.OrderBy(x => x.Start))
        {
            localFileStream.Seek(Segment.Start, SeekOrigin.Begin);

            using (Stream tempStream = new FileStream(Segment.FileLocation, FileMode.Open, FileAccess.Read))
            {
                await tempStream.CopyToAsync(localFileStream, cancellationToken);
            }
            File.Delete(Segment.FileLocation);
        }

        await Task.CompletedTask;
    }

}
