using PlainlyIpc.Tcp;
using System.Net;

namespace PlainlyIpc.IPC;

/// <summary>
/// Factory for IPC handlers
/// </summary>
public sealed partial class IpcFactory
{

    /// <summary>
    /// Creates a new TCP server based IPC receiver.
    /// </summary>
    /// <param name="ipEndPoint">The IP endpoint.</param>
    /// <returns>The IPC receiver instance.</returns>
    public Task<IIpcReceiver> CreateTcpIpcReceiver(IPEndPoint ipEndPoint)
    {
        ManagedTcpServer? managedTcpServer = null;
        IIpcReceiver? ipcReceiver = null;
        try
        {
            managedTcpServer = new(ipEndPoint);
            ipcReceiver = new IpcReceiver(managedTcpServer, objectConverter);
            _ = managedTcpServer.StartAsync();
            return Task.FromResult(ipcReceiver);
        }
        catch
        {
            ipcReceiver?.Dispose();
            managedTcpServer?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new TCP client based IPC sender.
    /// </summary>
    /// <param name="ipEndPoint">The IP endpoint.</param>
    /// <returns>The IPC sender instance.</returns>
    public async Task<IIpcSender> CreateTcpIpcSender(IPEndPoint ipEndPoint)
    {
        ManagedTcpClient? managedTcpClient = null;
        IIpcSender? ipcSender = null;
        try
        {
            managedTcpClient = new(ipEndPoint);
            ipcSender = new IpcSender(managedTcpClient, objectConverter);
            await managedTcpClient.ConnectAsync().ConfigureAwait(false);
            return ipcSender;
        }
        catch
        {
            ipcSender?.Dispose();
            managedTcpClient?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new TCP server based IPC handler.
    /// </summary>
    /// <param name="ipEndPoint">The IP endpoint.</param>
    /// <returns>The IPC handler instance.</returns>
    public Task<IIpcHandler> CreateTcpIpcServer(IPEndPoint ipEndPoint)
    {
        ManagedTcpServer? managedTcpServer = null;
        IIpcHandler? ipcHandler = null;
        try
        {
            managedTcpServer = new(ipEndPoint);
            ipcHandler = new IpcHandler(managedTcpServer, objectConverter);
            _ = managedTcpServer.StartAsync();
            return Task.FromResult(ipcHandler);
        }
        catch
        {
            ipcHandler?.Dispose();
            managedTcpServer?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a new TCP client based IPC handler.
    /// </summary>
    /// <param name="ipEndPoint">The IP endpoint.</param>
    /// <returns>The IPC handler instance.</returns>
    public async Task<IIpcHandler> CreateTcpIpcClient(IPEndPoint ipEndPoint)
    {
        ManagedTcpClient? managedTcpClient = null;
        IIpcHandler? ipcHandler = null;
        try
        {
            managedTcpClient = new(ipEndPoint);
            ipcHandler = new IpcHandler(managedTcpClient, objectConverter);
            await managedTcpClient.ConnectAsync().ConfigureAwait(false);
            _ = managedTcpClient.AcceptIncommingData();
            return ipcHandler;
        }
        catch
        {
            ipcHandler?.Dispose();
            managedTcpClient?.Dispose();
            throw;
        }
    }

}
