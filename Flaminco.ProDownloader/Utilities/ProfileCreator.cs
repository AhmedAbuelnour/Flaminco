using System.Net.Http.Headers;

namespace Flaminco.ProDownloader.Utilities;

public class ProfileCreator
{
    private readonly ResumableHttpClient _resumableHttpClient;
    private string _directoryPath;
    private string _fileName;
    private string _url;
    public ProfileCreator(ResumableHttpClient resumableHttpClient)
    {
        _resumableHttpClient = resumableHttpClient;
    }

    public void Initializer(string url, string directoryPath, string fileName)
    {
        _directoryPath = directoryPath;
        _fileName = fileName;
        _url = url;
    }

    /// <summary>
    /// Loads the downloader with the required information about the remote file.
    /// </summary>
    /// <returns>
    /// returns a profile, which contains all information required to download the file.
    /// </returns>
    public async Task<ServerFileProfile> CreateFileProfileAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_url)) throw new ArgumentNullException("Url", "Can't let Url to be empty!, Call Initializer");

        // Validation for https and http
        if (!(_url.StartsWith("https://") || _url.StartsWith("http://"))) throw new Exception("Only Support Http, Https protocols");

        if (string.IsNullOrWhiteSpace(_directoryPath)) throw new ArgumentNullException("Directoy Path", "Can't directory Path to be empty!, Call Initializer");

        if (string.IsNullOrWhiteSpace(_fileName)) throw new ArgumentNullException("File Name", "Can't file name to be empty!, Call Initializer");


        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                throw new Exception(httpResponseMessage.ReasonPhrase);
            }

            return new ServerFileProfile
            {
                Name = httpResponseMessage.Content.Headers?.ContentDisposition?.FileName ?? httpResponseMessage.RequestMessage.RequestUri.Segments.LastOrDefault(),
                MediaType = httpResponseMessage.Content.Headers.ContentType.MediaType,
                Size = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault(),
                Extension = httpResponseMessage.Content.Headers.ContentType.MediaType.GetFileExtension(),
                IsResumable = await _resumableHttpClient.IsResumableAsync(_url, cancellationToken),
                Stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken),
                TotalReadBytes = 0,
                FileLocation = $"{_directoryPath}/{_fileName}"
            };
        }
    }
    /// <summary>
    /// Loads the downloader with the required information about the remote file in segments.
    /// </summary>
    /// <returns>
    /// returns list of profiles, which contains all information required to download the file.
    /// </returns>
    public async IAsyncEnumerable<ServerSegmentProfile> CreateSegmentProfilesAsync(int SegmentNumbers = 8,
                                                                                   CancellationToken cancellationToken = default)
    {

        if (string.IsNullOrWhiteSpace(_url)) throw new ArgumentNullException("Url", "Can't let Url to be empty!");

        // Validation for https and http
        if (!(_url.StartsWith("https://") || _url.StartsWith("http://"))) throw new Exception("Only Support Http, Https protocols");

        if (string.IsNullOrWhiteSpace(_directoryPath)) throw new ArgumentNullException("Directoy Path", "Can't directory Path to be empty!, Call Initializer");

        if (string.IsNullOrWhiteSpace(_fileName)) throw new ArgumentNullException("File Name", "Can't file name to be empty!, Call Initializer");

        ServerFileProfile serverFileProfile = await CreateFileProfileAsync(cancellationToken);

        IEnumerable<(long Start, long End)> SegmentPosition(long ContentLength, int ChunksNumber)
        {
            long PartSize = (long)Math.Ceiling(ContentLength / (double)ChunksNumber);
            for (var i = 0; i < ChunksNumber; i++)
                yield return (i * PartSize + Math.Min(1, i), Math.Min((i + 1) * PartSize, ContentLength));
        }

        if (serverFileProfile.IsResumable)
        {
            foreach ((long Start, long End) in SegmentPosition(serverFileProfile.Size, SegmentNumbers))
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(Start, End);

                    // Sends Http Get Request
                    HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    if (httpResponseMessage.IsSuccessStatusCode == false)
                    {
                        throw new Exception(httpResponseMessage.ReasonPhrase);
                    }
                    yield return new ServerSegmentProfile
                    {
                        Size = httpResponseMessage.Content.Headers.ContentLength.GetValueOrDefault(-1),
                        Stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken),
                        TotalReadBytes = 0,
                        Start = Start,
                        End = End,
                        FileLocation = Path.GetTempFileName()
                    };
                }
            }
        }
        else
        {
            throw new Exception("Can't create segments for unresumable remote resource");
        }
    }


    public async Task ReconstructSegmentProfilesAsync(IEnumerable<ServerSegmentProfile> profiles, CancellationToken cancellationToken = default)
    {
        using (Stream localFileStream = new FileStream($"{_directoryPath}/{_fileName}", FileMode.Create, FileAccess.ReadWrite))
        {
            foreach (var Segment in profiles.OrderBy(x => x.Start))
            {
                localFileStream.Seek(Segment.Start, SeekOrigin.Begin);

                using (Stream tempStream = new FileStream(Segment.FileLocation, FileMode.Open, FileAccess.Read))
                {
                    await tempStream.CopyToAsync(localFileStream, cancellationToken);
                }
                File.Delete(Segment.FileLocation);
            }
        }
    }

}
