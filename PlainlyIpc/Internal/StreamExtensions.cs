using System.IO;
using System.Threading;

namespace PlainlyIpc.Internal;

internal static class StreamExtensions
{
    public static async Task ReadExactly(this Stream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
#if NETSTANDARD
            int read = await stream.ReadAsync(buffer, offset, count - offset).ConfigureAwait(false);
#else            
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset)).ConfigureAwait(false);
#endif
            if (read == 0) { throw new EndOfStreamException($"The end of the stream was reached before all {count} bytes were read."); }
            offset += read;
        }
    }

    public static async Task ReadExactly(this Stream stream, byte[] buffer, int count, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < count)
        {
#if NETSTANDARD
            int read = await stream.ReadAsync(buffer, offset, count - offset, cancellationToken).ConfigureAwait(false);
#else
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), cancellationToken).ConfigureAwait(false);
#endif
            if (read == 0) { throw new EndOfStreamException($"The end of the stream was reached before all {count} bytes were read."); }
            offset += read;
        }
    }

#if NETSTANDARD
    public static Task WriteAsync(this Stream stream, byte[] data)
    {
        return stream.WriteAsync(data, 0, data.Length);
    }
#endif


}
