using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Text.Json based IObjectConverter implementation.
/// </summary>
public sealed class JsonObjectConverter : IObjectConverter
{
    private readonly JsonSerializerOptions jsonSerializeOptions = new() { WriteIndented = true };
    private JsonSerializerOptions? jsonDeserializeOptions;

    /// <summary>
    /// Registers an implementation for an interface to deserialize interface instances.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <typeparam name="TImplementation">The type implementing the interface.</typeparam>
    public void AddInterfaceImplementation<TInterface, TImplementation>()
        where TImplementation : TInterface
    {
        jsonDeserializeOptions ??= new();
        jsonDeserializeOptions.Converters.Add(new TypeMappingConverter<TInterface, TImplementation>());
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? data)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, jsonSerializeOptions);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] data)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (jsonDeserializeOptions is not null)
        {
            return JsonSerializer.Deserialize<T>(data, jsonDeserializeOptions);
        }
        return JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        if (jsonDeserializeOptions is not null)
        {
            return JsonSerializer.Deserialize(data, type, jsonDeserializeOptions);
        }
        return JsonSerializer.Deserialize(data, type);
    }

    internal class TypeMappingConverter<TType, TImplementation> : JsonConverter<TType>
        where TImplementation : TType
    {
        public override TType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<TImplementation>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, options);
    }

}
