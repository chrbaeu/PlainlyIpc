using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Runtime.Serialization.Formatters.Binary based IObjectConverter implentation.
/// </summary>
#if NET6_0_OR_GREATER
#pragma warning disable SYSLIB0011
[Obsolete("Not recommended for productive use due to security risks.")]
#endif
public sealed class BinaryObjectConverter : IObjectConverter
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? data)
    {
        if (data is null) { return Array.Empty<byte>(); }
        BinaryFormatter serializer = new();
        using MemoryStream memStream = new();
        serializer.Serialize(memStream, data);
        return memStream.ToArray();
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] data)
    {
        if (data.Length == 0) { return default; }
        using MemoryStream memStream = new(data);
        BinaryFormatter serializer = new();
        return (T?)serializer.Deserialize(memStream);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data.Length == 0) { return default; }
        using MemoryStream memStream = new(data);
        BinaryFormatter serializer = new();
        return serializer.Deserialize(memStream);
    }

}
#if NET6_0_OR_GREATER
#pragma warning restore SYSLIB0011
#endif
