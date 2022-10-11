using PlainlyIpc.Interfaces;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PlainlyIpc.Converter;

public class BinaryObjectConverter : IObjectConverter
{
    public byte[] Serialize<T>(T? data)
    {
        if (data is null) { return new byte[0]; }
        BinaryFormatter serializer = new();
        using MemoryStream memStream = new();
        serializer.Serialize(memStream, data);
        return memStream.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        if (data.Length == 0) { return default; }
        using MemoryStream memStream = new(data);
        BinaryFormatter serializer = new();
        return (T?)serializer.Deserialize(memStream);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        if (data.Length == 0) { return default; }
        using MemoryStream memStream = new(data);
        BinaryFormatter serializer = new();
        return serializer.Deserialize(memStream);
    }

}
