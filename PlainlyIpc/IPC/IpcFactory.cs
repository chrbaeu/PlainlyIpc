using PlainlyIpc.Converter;

namespace PlainlyIpc.IPC;

/// <summary>
/// Factory for IPC handlers
/// </summary>
public sealed partial class IpcFactory
{
    private readonly IObjectConverter objectConverter;

    /// <summary>
    /// Creates a new IPC factory.
    /// </summary>
    public IpcFactory()
    {
        this.objectConverter = new JsonObjectConverter();
    }

    /// <summary>
    /// Creates a new IPC factory.
    /// </summary>
    /// <param name="objectConverter">Object converter to be used for serialization and deserialization.</param>
    public IpcFactory(IObjectConverter objectConverter)
    {
        this.objectConverter = objectConverter;
    }

}
