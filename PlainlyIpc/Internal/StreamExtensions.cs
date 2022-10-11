using System.IO;
using System.Threading.Tasks;

namespace PlainlyIpc.Internal;

internal static class StreamExtensions
{
    public static async Task ReadExactly(this Stream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer, offset, count - offset);
            if (read == 0) { throw new EndOfStreamException($"The end of the stream was reached before all {count} bytes were read."); }
            offset += read;
        }
    }
}
