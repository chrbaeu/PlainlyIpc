using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Text.Json based IObjectConverter implementation.
/// </summary>
public sealed class JsonObjectConverter : IObjectConverter
{
    private JsonSerializerOptions? jsonSerializerOptions;

    /// <summary>
    /// Registers an implementation for an interface to deserialize interface instances.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <typeparam name="TImplementation">The type implementing the interface.</typeparam>
    [Obsolete("Use AddInterfaceImplementation instead.")]
    public void AddInterfaceImplentation<TInterface, TImplementation>()
        where TImplementation : TInterface
    {
        AddInterfaceImplementation<TInterface, TImplementation>();
    }

    /// <summary>
    /// Registers an implementation for an interface to deserialize interface instances.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <typeparam name="TImplementation">The type implementing the interface.</typeparam>
    public void AddInterfaceImplementation<TInterface, TImplementation>()
        where TImplementation : TInterface
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new TypeMappingConverter<TInterface, TImplementation>());
    }

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
        if (jsonSerializerOptions is not null)
        {
            return JsonSerializer.Deserialize<T>(data, jsonSerializerOptions);
        }
        return JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        if (jsonSerializerOptions is not null)
        {
            return JsonSerializer.Deserialize(data, type, jsonSerializerOptions);
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
