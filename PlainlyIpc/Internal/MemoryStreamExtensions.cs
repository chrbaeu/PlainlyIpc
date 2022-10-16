using System.IO;

namespace PlainlyIpc.Internal;

internal static class MemoryStreamExtensions
{
    public static void WriteInt(this MemoryStream memoryStream, int val)
    {
        memoryStream.Write(BitConverter.GetBytes(val), 0, 4);
    }

    public static int ReadInt(this MemoryStream memoryStream)
    {
        byte[] intBytes = new byte[4];
        memoryStream.Read(intBytes, 0, 4);
        return BitConverter.ToInt32(intBytes, 0);
    }

    public static void WriteLong(this MemoryStream memoryStream, long val)
    {
        memoryStream.Write(BitConverter.GetBytes(val), 0, 8);
    }

    public static long ReadLong(this MemoryStream memoryStream)
    {
        byte[] callIdBytes = new byte[8];
        memoryStream.Read(callIdBytes, 0, 8);
        return BitConverter.ToInt64(callIdBytes, 0);
    }

    public static void WriteArray(this MemoryStream memoryStream, byte[] arr)
    {
        memoryStream.WriteInt(arr.Length);
        memoryStream.Write(arr, 0, arr.Length);
    }

    public static void WriteArrayArray(this MemoryStream memoryStream, byte[][] arr)
    {
        memoryStream.WriteInt(arr.Length);
        foreach (byte[] itemBytes in arr)
        {
            memoryStream.WriteArray(itemBytes);
        }
    }
    public static byte[] ReadArray(this MemoryStream memoryStream)
    {
        int payloadLength = memoryStream.ReadInt();
        byte[] payloadBytes = new byte[payloadLength];
        memoryStream.Read(payloadBytes, 0, payloadLength);
        return payloadBytes;
    }

    public static byte[][] ReadArrayArray(this MemoryStream memoryStream)
    {
        int arrayLength = memoryStream.ReadInt();
        byte[][] result = new byte[arrayLength][];
        for (int i = 0; i < arrayLength; i++)
        {
            result[i] = memoryStream.ReadArray();
        }
        return result;
    }

    public static void WriteUtf8String(this MemoryStream memoryStream, string str)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        memoryStream.Write(strBytes, 0, strBytes.Length);
        memoryStream.WriteByte(0);
    }

    public static string ReadUtf8String(this MemoryStream memoryStream)
    {
        long originalPosition = memoryStream.Position;
        while (memoryStream.ReadByte() != 0) { }
        long positionAfterReadingZero = memoryStream.Position;
        memoryStream.Seek(originalPosition, SeekOrigin.Begin);
        byte[] utf8Bytes = new byte[positionAfterReadingZero - originalPosition - 1];
        memoryStream.Read(utf8Bytes, 0, utf8Bytes.Length);
        memoryStream.ReadByte();
        return Encoding.UTF8.GetString(utf8Bytes);
    }

    public static byte[] ReadData(this MemoryStream memoryStream, int bytes)
    {
        byte[] payloadBytes = new byte[bytes];
        memoryStream.Read(payloadBytes, 0, bytes);
        return payloadBytes;
    }

#if NETSTANDARD

    public static void Write(this MemoryStream memoryStream, byte[] data)
    {
        memoryStream.Write(data, 0, data.Length);
    }

    public static void Write(this MemoryStream memoryStream, ReadOnlySpan<byte> data)
    {
        memoryStream.Write(data.ToArray(), 0, data.Length);
    }

#endif

}
