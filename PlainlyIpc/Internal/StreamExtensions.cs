using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PlainlyIpc.Internal;

internal static class StreamExtensions
{
    public static async Task ReadExactly(this Stream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
#if NET6_0_OR_GREATER
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset));
#else
            int read = await stream.ReadAsync(buffer, offset, count - offset);
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
#if NET6_0_OR_GREATER
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), cancellationToken);
#else
            int read = await stream.ReadAsync(buffer, offset, count - offset, cancellationToken);
#endif
            if (read == 0) { throw new EndOfStreamException($"The end of the stream was reached before all {count} bytes were read."); }
            offset += read;
        }
    }


}
