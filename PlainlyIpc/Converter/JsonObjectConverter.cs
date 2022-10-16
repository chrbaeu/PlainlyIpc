#if NET6_0_OR_GREATER

using System.Text.Json;

namespace PlainlyIpc.Converter;

public class JsonObjectConverter : IObjectConverter
{
    public byte[] Serialize<T>(T? data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.SerializeToUtf8Bytes(data, options);
    }

    public T? Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        return JsonSerializer.Deserialize(data, type);
    }

}

#endif
