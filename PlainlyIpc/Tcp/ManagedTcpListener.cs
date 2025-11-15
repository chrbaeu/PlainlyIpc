using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlainlyIpc.Tcp;

/// <summary>
/// Managed TCP listener class.
/// </summary>
internal sealed class ManagedTcpListener : IDisposable
{
    private readonly TcpListener tcpListener;
    private CancellationTokenSource cancellationTokenSource = new();
    private bool isDisposed;

    /// <summary>
    /// Gets the available IP addresses of the host.
    /// </summary>
    /// <returns>The list of available IP addresses.</returns>
    public static IList<IPAddress> GetHostIpAddresses() => Dns.GetHostEntry(Dns.GetHostName()).AddressList;

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
    /// Creates a new server socket to listen on a predefined port on any IP.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    public ManagedTcpListener(ushort port) => tcpListener = new TcpListener(IPAddress.Any, port);

    /// <summary>
    /// Creates a server socket to listen on a predefined ip and port.
    /// </summary>
    /// <param name="ipAddress">The IP to listen on</param>
    /// <param name="port">The port to listen on.</param>
    public ManagedTcpListener(IPAddress ipAddress, ushort port) => tcpListener = new TcpListener(ipAddress, port);

    /// <summary>
    /// Creates a server socket to listen on a predefined network endpoint.
    /// </summary>
    /// <param name="ipEndPoint">The network endpoint to listen on</param>
    public ManagedTcpListener(IPEndPoint ipEndPoint) => tcpListener = new TcpListener(ipEndPoint);

    /// <summary>
    /// Start asynchrone listening for connections.
    /// </summary>
    public Task StartListenAsync(int? backlog = null)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(ManagedTcpListener)); }
        if (IsListening) { return Task.CompletedTask; }
        if (backlog is null) { tcpListener.Start(); } else { tcpListener.Start(backlog.Value); }
        IsListening = true;
        return Task.Run(WaitForClient);
    }

    /// <summary>
    /// Stop listening for new clients.
    /// </summary>
    public void Stop()
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(ManagedTcpListener)); }
        if (!IsListening) { return; }
        IsListening = false;
        cancellationTokenSource.Cancel();
        tcpListener.Stop();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = new();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed) { return; }
        isDisposed = true;
        IsListening = false;
        cancellationTokenSource.Cancel();
        tcpListener.Stop();
#if NET8_0_OR_GREATER
        tcpListener.Dispose();
#endif
        cancellationTokenSource.Dispose();
        ErrorOccurred = null;
    }

    private async Task WaitForClient()
    {
        try
        {
            while (IsListening && !cancellationTokenSource.Token.IsCancellationRequested)
            {
#if NETSTANDARD2_0
                TcpClient tcpClient = await Task.Factory.StartNew(l => ((TcpListener)l).AcceptTcpClientAsync(), tcpListener,
                    cancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap().ConfigureAwait(false);
#else
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationTokenSource.Token).ConfigureAwait(false);
#endif
                IncomingTcpClientEventArgs eventArgs = new(new(tcpClient));
                _ = Task.Run(() => IncomingTcpClient?.Invoke(this, eventArgs)).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        ErrorOccurred?.Invoke(this, new(ErrorEventCode.EventHandlerError, "Calling the event handlers has thrown an exception.", x.Exception));
                    }
                }, TaskScheduler.Default);
            }
            IsListening = false;
        }
        catch (OperationCanceledException)
        {
            IsListening = false;
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new(ErrorEventCode.UnexpectedError, "Server stopped", e));
            IsListening = false;
        }
    }

}
