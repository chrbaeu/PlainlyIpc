#if NET6_0_OR_GREATER

using System.Text.Json;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Text.Json based IObjectConverter implentation.
/// </summary>
public class JsonObjectConverter : IObjectConverter
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.SerializeToUtf8Bytes(data, options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        return JsonSerializer.Deserialize(data, type);
    }

}

#endif
