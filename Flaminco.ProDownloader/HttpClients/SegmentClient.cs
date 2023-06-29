using Flaminco.ProDownloader.Models;
using System.IO.Pipelines;
using System.Net.Http.Headers;

namespace Flaminco.ProDownloader.HttpClients
{
    internal sealed class SegmentClient
    {
        private readonly HttpClient _httpClient;
        private readonly Pipe _pipeline;
        internal SegmentClient(HttpClient httpClient, Pipe pipeline)
        {
            _httpClient = httpClient;
            _pipeline = pipeline;
        }

        internal async Task DownloadAsync(SegmentMetadata segment, Action<long> downloadProgressCallback, CancellationToken cancellationToken)
        {
            await Task.WhenAll(WriteInPipeAsync(segment, downloadProgressCallback, cancellationToken), ReadFromPipeAsync(segment, cancellationToken));
        }

        internal async Task<Stream?> DownloadSegmentAsync(SegmentMetadata segment, CancellationToken cancellationToken = default)
        {
            using (Stream localFileStream = new FileStream(segment.TempPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                long startPosition = (localFileStream.Length + segment.Start);

                if (startPosition >= segment.End)
                {
                    return await Task.FromResult<Stream>(null);
                }

                segment.TotalReadBytes = localFileStream.Length;

                _httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(startPosition, segment.End);
            }

            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(segment.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
        }

        internal async Task WriteInPipeAsync(SegmentMetadata segment, Action<long> downloadProgressCallback, CancellationToken cancellationToken = default)
        {
            int bytesRead;
            bool firstTime = true;
            using (Stream? ContentStream = await DownloadSegmentAsync(segment, cancellationToken))
            {
                if (ContentStream is not null)
                {
                    while ((bytesRead = await ContentStream.ReadAsync(_pipeline.Writer.GetMemory(), cancellationToken)) > 0) // Where the downloading part is happening
                    {
                        // Tell the PipeWriter how much was read from the Socket.
                        _pipeline.Writer.Advance(bytesRead);

                        if (firstTime)
                        {
                            downloadProgressCallback.Invoke(segment.TotalReadBytes + bytesRead);
                            firstTime = false;
                        }
                        else
                        {
                            downloadProgressCallback.Invoke(bytesRead);
                        }

                        // Make the data available to the PipeReader.
                        FlushResult flushResult = await _pipeline.Writer.FlushAsync(cancellationToken);

                        if (flushResult.IsCanceled || flushResult.IsCompleted) break;
                    }
                }
                else
                {
                    await Task.CompletedTask;
                }

                // By completing PipeWriter, tell the PipeReader that there's no more data coming.
                await _pipeline.Writer.CompleteAsync();
            }
        }

        internal async Task ReadFromPipeAsync(SegmentMetadata segment, CancellationToken cancellationToken = default)
        {
            using (Stream LocalFile = new FileStream(segment.TempPath, FileMode.Append, FileAccess.Write))
            {
                if (LocalFile.Length < segment.End)
                {
                    LocalFile.Seek(LocalFile.Length, SeekOrigin.Begin);

                    while (true)
                    {
                        ReadResult ReadResult = await _pipeline.Reader.ReadAsync(cancellationToken);

                        foreach (ReadOnlyMemory<byte> segmentBuffer in ReadResult.Buffer)
                            await LocalFile.WriteAsync(segmentBuffer, cancellationToken);

                        // Tell the PipeReader how much of the buffer has been consumed.
                        _pipeline.Reader.AdvanceTo(ReadResult.Buffer.End);

                        // Stop reading if there's no more data coming.
                        if (ReadResult.IsCompleted || ReadResult.IsCanceled) break;
                    }
                }
                else
                {
                    await Task.CompletedTask;
                }

                // Mark the PipeReader as complete.
                await _pipeline.Reader.CompleteAsync();
            }
        }
    }
}
