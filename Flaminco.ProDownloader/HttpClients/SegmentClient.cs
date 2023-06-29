using Flaminco.ProDownloader.Models;
using System.IO.Pipelines;
using System.Net.Http.Headers;

namespace Flaminco.ProDownloader.HttpClients
{
    internal sealed class SegmentClient
    {
        private readonly HttpClient _httpClient;
        private readonly Pipe _pipeline;
        public SegmentClient(HttpClient httpClient, Pipe pipeline)
        {
            _httpClient = httpClient;
            _pipeline = pipeline;
        }

        internal async Task DownloadAsync(SegmentMetadata segment, Action<int> downloadProgressCallback, CancellationToken cancellationToken)
        {
            await Task.WhenAll(WriteInPipeAsync(segment, downloadProgressCallback, cancellationToken), ReadFromPipeAsync(segment, cancellationToken));
        }

        internal async Task<Stream> DownloadSegmentAsync(SegmentMetadata segment, CancellationToken cancellationToken = default)
        {
            using (Stream localFileStream = new FileStream(segment.TempPath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                _httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(localFileStream.Length > 0 ? localFileStream.Length : segment.Start, segment.End);
            }

            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(segment.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
        }

        internal async Task WriteInPipeAsync(SegmentMetadata segment, Action<int> downloadProgressCallback, CancellationToken cancellationToken = default)
        {
            int bytesRead;

            using (Stream ContentStream = await DownloadSegmentAsync(segment, cancellationToken))
            {
                while ((bytesRead = await ContentStream.ReadAsync(_pipeline.Writer.GetMemory(), cancellationToken)) > 0) // Where the downloading part is happening
                {
                    // Tell the PipeWriter how much was read from the Socket.
                    _pipeline.Writer.Advance(bytesRead);

                    downloadProgressCallback.Invoke(bytesRead);

                    // Make the data available to the PipeReader.
                    FlushResult flushResult = await _pipeline.Writer.FlushAsync(cancellationToken);

                    if (flushResult.IsCanceled || flushResult.IsCompleted) break;
                }
            }
            // By completing PipeWriter, tell the PipeReader that there's no more data coming.
            await _pipeline.Writer.CompleteAsync();
        }

        internal async Task ReadFromPipeAsync(SegmentMetadata segment, CancellationToken cancellationToken = default)
        {
            using (Stream LocalFile = new FileStream(segment.TempPath, FileMode.Append, FileAccess.Write))
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
            // Mark the PipeReader as complete.
            await _pipeline.Reader.CompleteAsync();
        }
    }
}
