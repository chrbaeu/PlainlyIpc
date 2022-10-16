namespace PlainlyIpc.Interfaces;

public interface IObjectConverter
{
    public byte[] Serialize<T>(T? data);
    public T? Deserialize<T>(byte[] data);
    public object? Deserialize(byte[] data, Type type);

}
