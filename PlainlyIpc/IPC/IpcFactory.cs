using PlainlyIpc.NamedPipe;

namespace PlainlyIpc.IPC;

/// <summary>
/// Factory for IPC handlers
/// </summary>
public class IpcFactory
{
    private readonly IObjectConverter objectConverter;

    /// <summary>
    /// Creates a new IPC factory.
    /// </summary>
    /// <param name="objectConverter">Object converter to be used for serialization and deserialization.</param>
    public IpcFactory(IObjectConverter objectConverter)
    {
        this.objectConverter = objectConverter;
    }

    /// <summary>
    /// Creates a new named pipe server based IPC handler.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC handler instance.</returns>
    public Task<IIpcHandler> CreateNampedPipeIpcServer(string namedPipeName)
    {
        NamedPipeServer? namedPipeServer = null;
        IIpcHandler? ipcHandler = null;
        try
        {
            namedPipeServer = new(namedPipeName);
            ipcHandler = new IpcHandler(namedPipeServer, namedPipeServer, objectConverter);
            _ = namedPipeServer.StartAsync();
            return Task.FromResult(ipcHandler);
        }
        catch
        {
            ipcHandler?.Dispose();
            namedPipeServer?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new named pipe client based IPC handler.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC handler instance.</returns>
    public async Task<IIpcHandler> CreateNampedPipeIpcClient(string namedPipeName)
    {
        NamedPipeClient? namedPipeClient = null;
        IIpcHandler? ipcHandler = null;
        try
        {
            namedPipeClient = new(namedPipeName);
            ipcHandler = new IpcHandler(namedPipeClient, namedPipeClient, objectConverter);
            await namedPipeClient.ConnectAsync();
            return ipcHandler;
        }
        catch
        {
            ipcHandler?.Dispose();
            namedPipeClient?.Dispose();
            throw;
        }
    }

}
