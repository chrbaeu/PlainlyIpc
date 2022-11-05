using System.Text.Json;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Text.Json based IObjectConverter implentation.
/// </summary>
public sealed class JsonObjectConverter : IObjectConverter
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
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        return JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        return JsonSerializer.Deserialize(data, type);
    }

}
