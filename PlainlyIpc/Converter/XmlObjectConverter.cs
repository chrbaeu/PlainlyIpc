using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace PlainlyIpc.Converter;

/// <summary>
/// System.Xml.Serialization based IObjectConverter implentation.
/// </summary>
[Obsolete("Only supports serializable types.")]
public sealed class XmlObjectConverter : IObjectConverter
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? data)
    {
        XmlSerializer serializer = new(typeof(Container<T>));
        using MemoryStream memStream = new();
        serializer.Serialize(memStream, new Container<T>() { Value = data });
        return memStream.ToArray();
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] data)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        using MemoryStream memStream = new(data);
        using var xmlReader = XmlReader.Create(memStream);
        XmlSerializer xmlSerializer = new(typeof(Container<T>));
        Container<T> container = (Container<T>)xmlSerializer.Deserialize(xmlReader)!;
        return container.Value;
    }

    /// <inheritdoc/>
    public object? Deserialize(byte[] data, Type type)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        MethodInfo method = typeof(XmlObjectConverter).GetMethod(nameof(XmlObjectConverter.Deserialize), new Type[] { typeof(byte[]) })!;
        MethodInfo generic = method.MakeGenericMethod(type);
        return generic.Invoke(this, new object[] { data });
    }

    /// <summary>
    /// Internal used container class
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible",
        Justification = "Only internally used wrapper for serialization and deserialization, but it must be public because this is required by the XmlSerializer.")]
    public sealed class Container<T>
    {
        /// <summary>
        /// Value of the container.
        /// </summary>
        public T? Value { get; set; }
    }

}
