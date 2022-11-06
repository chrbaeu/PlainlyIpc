using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlainlyIpc.Tcp;

/// <summary>
/// Managed TCP client class.
/// </summary>
internal sealed class ManagedTcpClient : IDataHandler
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly TcpClient tcpClient;
    private NetworkStream? networkStream;
    private bool isDisposed;

    ///<summary>
    ///True if this socket is connected to a server, false otherwise.
    ///</summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// The network endpoint used by the client.
    /// </summary>
    public IPEndPoint Endpoint { get; }

    /// <summary>
    /// Event called when data arrives.
    /// </summary>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Event called when a error occurs.
    /// </summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <summary>
    /// Create a socket using given IP address/hostname and port.
    /// </summary>
    /// <param name="endpoint">The IP address/hostname of the server to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    public ManagedTcpClient(string endpoint, ushort port)
    {
        if (!IPAddress.TryParse(endpoint, out IPAddress? address))
        {
            IPAddress[] addresses = Dns.GetHostAddresses(endpoint);
            if (addresses == null || addresses.Length == 0)
            {
                throw new ArgumentException("The given endpoint is not a valid IP address or hostname.", nameof(endpoint));
            }
            address = addresses[0];
        }
        Endpoint = new IPEndPoint(address, port);
        tcpClient = new();
    }

    /// <summary>
    /// Create a new socket using a given IP address and port.
    /// </summary>
    /// <param name="ipAddress">The IP address of the server to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    public ManagedTcpClient(IPAddress ipAddress, ushort port)
    {
        Endpoint = new IPEndPoint(ipAddress, port);
        tcpClient = new();
    }

    /// <summary>
    /// Create a new socket using a given network endpoint.
    /// </summary>
    /// <param name="endpoint">The network endpoint of the server to connect to.</param>
    public ManagedTcpClient(IPEndPoint endpoint)
    {
        Endpoint = endpoint;
        tcpClient = new();
    }

    /// <summary>
    /// Create a new socket based of an already existing connection
    /// </summary>
    /// <param name="tcpClient"></param>
    public ManagedTcpClient(TcpClient tcpClient)
    {
        if (tcpClient is null) { throw new ArgumentNullException(nameof(tcpClient)); }
        if (!tcpClient.Connected)
        {
            throw new ArgumentException("Socket is not connected!", nameof(tcpClient));
        }
        if (tcpClient.Client.RemoteEndPoint is not IPEndPoint remoteEndPoint || tcpClient.Client.LocalEndPoint is not IPEndPoint localEndPoint)
        {
            throw new ArgumentException("Network endpoints of the socket could not be identified!", nameof(tcpClient));
        }
        Endpoint = new IPEndPoint(remoteEndPoint.Address, localEndPoint.Port);
        this.tcpClient = tcpClient;
        networkStream = tcpClient.GetStream();
        IsConnected = true;
        _ = AcceptIncommingData();
    }

    /// <summary>
    /// Connect to the server.
    /// </summary>
    public async Task ConnectAsync(int connectionTimeout = 5000)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(ManagedTcpClient)); }
        if (IsConnected) { return; }
        try
        {
#if NETSTANDARD
            await tcpClient.ConnectAsync(Endpoint.Address, Endpoint.Port).WaitAsync(new(0, 0, 0, 0, connectionTimeout)).ConfigureAwait(false);
#else
            using CancellationTokenSource cts = new(connectionTimeout);
            await tcpClient.ConnectAsync(Endpoint.Address, Endpoint.Port, cts.Token).ConfigureAwait(false);
#endif
        }
        catch (OperationCanceledException e)
        {
            throw new TimeoutException($"Connecting to the endpoint has exceeded the timeout of {connectionTimeout}ms.", e);
        }
        networkStream = tcpClient.GetStream();
        IsConnected = true;
        _ = AcceptIncommingData();
    }

    /// <summary>
    /// Send data to the connected server/client
    /// </summary>
    /// <param name="data">The memory of bytes to send.</param>
    public async Task SendAsync(byte[] data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(ManagedTcpClient)); }
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (!IsConnected || networkStream is null) { throw new InvalidOperationException($"{nameof(ManagedTcpClient)} must be connected to send data!"); }
        try
        {
            await networkStream.WriteAsync(BitConverter.GetBytes(data.Length)).ConfigureAwait(false);
            await networkStream.WriteAsync(data).ConfigureAwait(false);
        }
        catch (Exception)
        {
            IsConnected = false;
            throw;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed) { return; }
        isDisposed = true;
        IsConnected = false;
        cancellationTokenSource.Cancel();
        networkStream?.Dispose();
        tcpClient.Dispose();
        cancellationTokenSource.Dispose();
        DataReceived = null;
        ErrorOccurred = null;
        GC.SuppressFinalize(this);
    }

    private async Task AcceptIncommingData()
    {
        if (networkStream is null) { throw new InvalidOperationException("AcceptIncommingData requires a open connection."); }
        try
        {
            byte[] lenArray = new byte[4];
            while (IsConnected && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await networkStream.ReadExactly(lenArray, 4, cancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch (EndOfStreamException e)
                {
                    IsConnected = false;
                    ErrorOccurred?.Invoke(this, new(ErrorEventCode.ConnectionLost, "The connection was lost.", e));
                    break;
                }
                int dataLen = BitConverter.ToInt32(lenArray, 0);
                byte[] dataArray = new byte[dataLen];
                await networkStream.ReadExactly(dataArray, dataLen, cancellationTokenSource.Token).ConfigureAwait(false);
                DataReceivedEventArgs eventArgs = new(dataArray);
                _ = Task.Run(() => DataReceived?.Invoke(this, eventArgs)).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        ErrorOccurred?.Invoke(this, new(ErrorEventCode.EventHandlerError, "Calling the event handlers has thrown an exception.", x.Exception));
                    }
                }, TaskScheduler.Default);
            }
        }
        catch (OperationCanceledException)
        {
            IsConnected = false;
        }
        catch (Exception e)
        {
            IsConnected = false;
            ErrorOccurred?.Invoke(this, new(ErrorEventCode.UnexpectedError, "An error occurred while receiving data. The connection was lost.", e));
        }
    }

}
