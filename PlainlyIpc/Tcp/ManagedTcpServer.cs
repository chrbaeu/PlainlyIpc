using System.Net;

namespace PlainlyIpc.Tcp;

/// <summary>
/// TCP server based on the ManagedTcpLister that implenets the IDataHandler interface.
/// </summary>
public class ManagedTcpServer : IDataHandler
{
    private readonly ManagedTcpListener tcpListener;
    private readonly List<ManagedTcpClient> clients = new();

    /// <inheritdoc/>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <inheritdoc/>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <summary>
    /// Indicates if the named pipe server listens for incoming data. 
    /// </summary>
    public bool IsActive { get; private set; }

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
        if (IsActive) { return Task.CompletedTask; }
        IsActive = true;
        return Task.Run(() => tcpListener.StartListenAync());
    }

    /// <inheritdoc/>
    public Task SendAsync(byte[] data)
    {
        return Task.WhenAll(clients.Select(x => x.SendAsync(data)).ToArray());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        tcpListener.Stop();
        GC.SuppressFinalize(this);
    }

    private void TcpListener_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

    private void TcpListener_IncomingTcpClient(object sender, IncomingTcpClientEventArgs e)
    {
        e.TcpClient.DataReceived += TcpClient_DataReceived;
        e.TcpClient.ErrorOccurred += TcpClient_ErrorOccurred;
        clients.Add(e.TcpClient);
        _ = e.TcpClient.AcceptIncommingData();
    }

    private void TcpClient_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
    {
        if (sender is ManagedTcpClient client)
        {
            clients.Remove(client);
        }
        ErrorOccurred?.Invoke(sender, e);
    }

    private void TcpClient_DataReceived(object sender, DataReceivedEventArgs e)
    {
        DataReceived?.Invoke(sender, e);
    }
}
