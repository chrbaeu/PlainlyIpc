namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for converter used for object serialization and deserialization.
/// </summary>
public interface IObjectConverter
{
    /// <summary>
    /// Serializes data of type T to a byte array.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <returns>A byte array representation of the data.</returns>
    public byte[] Serialize<T>(T? data);

    /// <summary>
    /// Deserializes a byte array to an object of type T.
    /// </summary>
    /// <param name="data">The byte array representation of the object.</param>
    /// <returns>The deserialized object.</returns>
    public T? Deserialize<T>(byte[] data);

    /// <summary>
    /// Deserializes a byte array to an object of the given type.
    /// </summary>
    /// <param name="data">The byte array representation of the object.</param>
    /// <param name="type">The type of the object.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(byte[] data, Type type);

}
