using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlainlyIpc.Tcp;

public sealed class MangedTcpListener
{
    private readonly object lockObject = new();
    private readonly TcpListener tcpListener;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    private bool isStopping;

    /// <summary>
    /// Indicates if the server is listening for connections.
    /// </summary>
    public bool IsListening { get; private set; }

    /// <summary>
    /// Event called when a client is connecting to the server.
    /// </summary>
    public event EventHandler<IncomingTcpClientEventArgs>? IncomingTcpClient;

    /// <summary>
    /// Event called when a error occurs.
    /// </summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <summary>
    /// Gets the available IP addresses of the host.
    /// </summary>
    /// <returns>The list of available IP addresses.</returns>
    public static IList<IPAddress> GetHostIpAddresses() => Dns.GetHostEntry(Dns.GetHostName()).AddressList;

    /// <summary>
    /// Creates a new server socket to listen on a predefined port on any IP.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    public MangedTcpListener(ushort port) => tcpListener = new TcpListener(IPAddress.Any, port);

    /// <summary>
    /// Creates a server socket to listen on a predefined ip and port.
    /// </summary>
    /// <param name="ipAddress">The IP to listen on</param>
    /// <param name="port">The port to listen on.</param>
    public MangedTcpListener(IPAddress ipAddress, ushort port) => tcpListener = new TcpListener(ipAddress, port);

    /// <summary>
    /// Creates a server socket to listen on a predefined network endpoint.
    /// </summary>
    /// <param name="ipEndPoint">The network endpoint to listen on</param>
    public MangedTcpListener(IPEndPoint ipEndPoint) => tcpListener = new TcpListener(ipEndPoint);

    /// <summary>
    /// Start asynchrone listening for connections.
    /// </summary>
    public Task StartListenAync(int? backlog = null)
    {
        lock (lockObject)
        {
            if (IsListening) { return Task.CompletedTask; }
            IsListening = true;
            if (backlog is null) { tcpListener.Start(); } else { tcpListener.Start(backlog.Value); }
        }
        return WaitForClient();
    }

    /// <summary>
    /// Stop listening for new clients.
    /// </summary>
    public void Stop() => Stop(null);

    private void Stop(Exception? e = null)
    {
        lock (lockObject)
        {
            if (!IsListening || isStopping) { return; }
            isStopping = true;
            cancellationTokenSource.Cancel();
            tcpListener.Stop();
            IsListening = false;
            isStopping = false;
        }
        if (e is not null)
        {
            ErrorOccurred?.Invoke(this, new(0, "Server stopped", e));
        }
    }

    [DebuggerNonUserCode]
    private async Task WaitForClient()
    {
        try
        {
            while (IsListening && !isStopping)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                IncomingTcpClient?.Invoke(this, new(new(tcpClient)));
            }
        }
        catch (OperationCanceledException)
        {
            Stop();
        }
        catch (Exception e)
        {
            Stop(e);
        }
    }

}
