using System.Text.Json;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Text.Json based IObjectConverter implentation.
/// </summary>
public sealed class JsonObjectConverter : IObjectConverter
{
    private readonly Dictionary<Type, Type> interfaceToImplementationDict = new();

    /// <summary>
    /// Registers an implementation for an interface to deserialize interface instances.
    /// </summary>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <typeparam name="TImplemntation">The type implementing the interface.</typeparam>
    public void AddInterfaceImplentation<TInterface, TImplemntation>()
        where TImplemntation : TInterface
    {
        interfaceToImplementationDict[typeof(TInterface)] = typeof(TImplemntation);
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
        if (interfaceToImplementationDict.TryGetValue(typeof(T), out var implType))
        {
            return (T?)JsonSerializer.Deserialize(data, implType);
        }
        return JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        if (interfaceToImplementationDict.TryGetValue(type, out var implType))
        {
            return JsonSerializer.Deserialize(data, implType);
        }
        return JsonSerializer.Deserialize(data, type);
    }

}
