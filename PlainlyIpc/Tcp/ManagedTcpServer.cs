using System.Net;

namespace PlainlyIpc.Tcp;

/// <summary>
/// TCP server based on the ManagedTcpLister that implenets the IDataHandler interface.
/// </summary>
internal sealed class ManagedTcpServer : IDataHandler
{
    private readonly ManagedTcpListener tcpListener;
    private readonly List<ManagedTcpClient> clients = new();
    private bool isDisposed;

    /// <inheritdoc/>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <inheritdoc/>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <summary>
    /// Indicates if the named pipe server listens for incoming data. 
    /// </summary>
    public bool IsActive => tcpListener.IsListening;

    /// <summary>
    /// Creates a new ManagedTcpServer for the given IP endpoint.
    /// </summary>
    /// <param name="ipEndPoint">The IP endpoint.</param>
    public ManagedTcpServer(IPEndPoint ipEndPoint)
    {
        tcpListener = new(ipEndPoint);
        tcpListener.IncomingTcpClient += TcpListener_IncomingTcpClient;
        tcpListener.ErrorOccurred += TcpListener_ErrorOccurred;
    }

    /// <summary>
    /// Starts the TCP server instance.
    /// </summary>
    public Task StartAsync()
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(ManagedTcpServer)); }
        if (IsActive) { return Task.CompletedTask; }
        return tcpListener.StartListenAync();
    }

    /// <inheritdoc/>
    public async Task SendAsync(byte[] data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(ManagedTcpServer)); }
        if (!clients.Any()) { throw new InvalidOperationException("There are no clients connected to which data can be sent."); }
        await Task.WhenAll(clients.ToList().Select(x => x.SendAsync(data)).ToArray()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed) { return; }
        isDisposed = true;
        tcpListener.Stop();
        tcpListener.Dispose();
        GC.SuppressFinalize(this);
    }

    private void TcpListener_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        ErrorOccurred?.Invoke(sender, e);
    }

    private void TcpListener_IncomingTcpClient(object? sender, IncomingTcpClientEventArgs e)
    {
        e.TcpClient.DataReceived += TcpClient_DataReceived;
        e.TcpClient.ErrorOccurred += TcpClient_ErrorOccurred;
        clients.Add(e.TcpClient);
    }

    private void TcpClient_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        if (sender is ManagedTcpClient client)
        {
            clients.Remove(client);
        }
        ErrorOccurred?.Invoke(sender, e);
    }

    private void TcpClient_DataReceived(object? sender, DataReceivedEventArgs e)
    {
        DataReceived?.Invoke(sender, e);
    }
}
