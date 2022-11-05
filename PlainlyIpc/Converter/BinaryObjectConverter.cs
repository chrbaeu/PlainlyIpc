using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Runtime.Serialization.Formatters.Binary based IObjectConverter implentation.
/// </summary>
#pragma warning disable CA2300 // Do not use insecure deserializer BinaryFormatter
#pragma warning disable CA2301 // Do not call BinaryFormatter.Deserialize without first setting BinaryFormatter.Binder
#if NET6_0_OR_GREATER
#pragma warning disable SYSLIB0011
#endif
[Obsolete("Not recommended for productive use due to security risks.")]
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
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (data.Length == 0) { return default; }
        using MemoryStream memStream = new(data);
        BinaryFormatter serializer = new();
        return (T?)serializer.Deserialize(memStream);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        if (data.Length == 0) { return default; }
        using MemoryStream memStream = new(data);
        BinaryFormatter serializer = new();
        return serializer.Deserialize(memStream);
    }

}
#if NET6_0_OR_GREATER
#pragma warning restore SYSLIB0011
#endif
#pragma warning restore CA2300 // Do not use insecure deserializer BinaryFormatter
#pragma warning restore CA2301 // Do not call BinaryFormatter.Deserialize without first setting BinaryFormatter.Binder
