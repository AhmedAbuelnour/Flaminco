using System.IO.Pipelines;

namespace Flaminco.ProDownloader.Utilities;

public class PipeController
{
    private readonly Pipe _pipeline;
    public PipeController()
    {
        _pipeline = new Pipe();
    }

    public async Task WriteInPipeAsync(BaseProfile profile, Action updateCallback, CancellationToken cancellationToken = default)
    {
        int bytesRead;
        while ((bytesRead = await profile.Stream.ReadAsync(_pipeline.Writer.GetMemory(), cancellationToken)) > 0) // Where the downloading part is happening
        {
            profile.TotalReadBytes += bytesRead;

            // Tell the PipeWriter how much was read from the Socket.
            _pipeline.Writer.Advance(bytesRead);

            updateCallback.Invoke();

            // Make the data available to the PipeReader.
            FlushResult flushResult = await _pipeline.Writer.FlushAsync(cancellationToken);

            if (flushResult.IsCanceled || flushResult.IsCompleted) break;
        }
        // By completing PipeWriter, tell the PipeReader that there's no more data coming.
        await _pipeline.Writer.CompleteAsync();
    }
    public async Task ReadFromPipeAsync(BaseProfile profile, CancellationToken cancellationToken = default)
    {
        using (Stream LocalFile = new FileStream(profile.FileLocation, FileMode.Append, FileAccess.Write))
        {
            LocalFile.Seek(LocalFile.Length, SeekOrigin.Begin);

            while (true)
            {
                ReadResult ReadResult = await _pipeline.Reader.ReadAsync(cancellationToken);

                foreach (ReadOnlyMemory<byte> segment in ReadResult.Buffer)
                    await LocalFile.WriteAsync(segment, cancellationToken);

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
