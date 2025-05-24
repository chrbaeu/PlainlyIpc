using PlainlyIpc.NamedPipe;

namespace PlainlyIpc.IPC;

/// <summary>
/// Factory for IPC handlers
/// </summary>
public sealed partial class IpcFactory
{
    /// <summary>
    /// Creates a new named pipe server based IPC receiver.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC receiver instance.</returns>
    public Task<IIpcReceiver> CreateNamedPipeIpcReceiver(string namedPipeName)
    {
        NamedPipeServer? namedPipeServer = null;
        IIpcReceiver? ipcReceiver = null;
        try
        {
            namedPipeServer = new(namedPipeName);
            ipcReceiver = new IpcReceiver(namedPipeServer, objectConverter);
            _ = namedPipeServer.StartAsync();
            return Task.FromResult(ipcReceiver);
        }
        catch
        {
            ipcReceiver?.Dispose();
            namedPipeServer?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new named pipe client based IPC sender.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC sender instance.</returns>
    public async Task<IIpcSender> CreateNamedPipeIpcSender(string namedPipeName)
    {
        NamedPipeClient? namedPipeClient = null;
        IIpcSender? ipcSender = null;
        try
        {
            namedPipeClient = new(namedPipeName);
            ipcSender = new IpcSender(namedPipeClient, objectConverter);
            await namedPipeClient.ConnectAsync().ConfigureAwait(false);
            return ipcSender;
        }
        catch
        {
            ipcSender?.Dispose();
            namedPipeClient?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new named pipe client based IPC sender.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC sender instance.</returns>
    [Obsolete("Use CreateNamedPipeIpcSender instead.")]
    public Task<IIpcSender> CreateNampedPipeIpcSender(string namedPipeName)
    {
        return CreateNamedPipeIpcSender(namedPipeName);
    }

    /// <summary>
    /// Creates a new named pipe server based IPC handler.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC handler instance.</returns>
    public Task<IIpcHandler> CreateNamedPipeIpcServer(string namedPipeName)
    {
        NamedPipeServer? namedPipeServer = null;
        IIpcHandler? ipcHandler = null;
        try
        {
            namedPipeServer = new(namedPipeName);
            ipcHandler = new IpcHandler(namedPipeServer, objectConverter);
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
    /// Creates a new named pipe server based IPC handler.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC handler instance.</returns>
    [Obsolete("Use CreateNamedPipeIpcServer instead.")]
    public Task<IIpcHandler> CreateNampedPipeIpcServer(string namedPipeName)
    {
        return CreateNamedPipeIpcServer(namedPipeName);
    }

    /// <summary>
    /// Creates a new named pipe client based IPC handler.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC handler instance.</returns>
    public async Task<IIpcHandler> CreateNamedPipeIpcClient(string namedPipeName)
    {
        NamedPipeClient? namedPipeClient = null;
        IIpcHandler? ipcHandler = null;
        try
        {
            namedPipeClient = new(namedPipeName);
            ipcHandler = new IpcHandler(namedPipeClient, objectConverter);
            await namedPipeClient.ConnectAsync().ConfigureAwait(false);
            return ipcHandler;
        }
        catch
        {
            ipcHandler?.Dispose();
            namedPipeClient?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new named pipe client based IPC handler.
    /// </summary>
    /// <param name="namedPipeName">The name of the named pipe.</param>
    /// <returns>The IPC handler instance.</returns>
    [Obsolete("Use CreateNamedPipeIpcClient instead.")]
    public Task<IIpcHandler> CreateNampedPipeIpcClient(string namedPipeName)
    {
        return CreateNamedPipeIpcClient(namedPipeName);
    }

}
