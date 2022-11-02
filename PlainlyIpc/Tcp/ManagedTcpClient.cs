using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlainlyIpc.Tcp;

public sealed class ManagedTcpClient : IDataHandler
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly TcpClient tcpClient;

    private NetworkStream? networkStream;
    private bool isClosing;

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
    /// Create a new socket based off an already existing connection
    /// </summary>
    /// <param name="tcpClient"></param>
    public ManagedTcpClient(TcpClient tcpClient)
    {
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
    }

    /// <summary>
    /// Connect to the server.
    /// </summary>
    public async Task ConnectAsync(int connectionTimeout = 1000)
    {
        if (IsConnected) { return; }
        try
        {
#if NETSTANDARD
            await tcpClient.ConnectAsync(Endpoint.Address, Endpoint.Port).WaitAsync(new(0, 0, 0, 0, connectionTimeout));
#else
            CancellationTokenSource cts = new(connectionTimeout);
            await tcpClient.ConnectAsync(Endpoint.Address, Endpoint.Port, cts.Token);
#endif
        }
        catch (OperationCanceledException e)
        {
            throw new TimeoutException($"Connecting to the endpoint has exceeded the timeout of {connectionTimeout}ms.", e);
        }
        IsConnected = true;
        networkStream = tcpClient.GetStream();
    }

    public async Task AcceptIncommingData()
    {
        if (!IsConnected || networkStream is null) { throw new InvalidOperationException("AcceptIncommingData requires a open connection."); }
        try
        {
            byte[] lenArray = new byte[4];
            while (IsConnected && !isClosing)
            {
                await networkStream.ReadExactly(lenArray, 4, cancellationTokenSource.Token);
                var dataLen = BitConverter.ToInt32(lenArray, 0);
                if (dataLen == -1)
                {
                    IsConnected = false;
                    break;
                }
                byte[] dataArray = new byte[dataLen];
                DataReceivedEventArgs dataReceivedEventArgs = new(dataArray);
                await networkStream.ReadExactly(dataArray, dataLen, cancellationTokenSource.Token);
                DataReceived?.Invoke(this, dataReceivedEventArgs);
            }
            Disconnect();
        }
        catch (EndOfStreamException)
        {
            Disconnect();
        }
        catch (OperationCanceledException)
        {
            Disconnect();
        }
        catch (Exception e)
        {
            Disconnect("Connection lost.", e);
        }
    }

    /// <summary>
    /// Disconnect from the remote host.
    /// </summary>
    public void Disconnect()
    {
        Disconnect("Connection closed.");
    }

    public void Disconnect(string reason, Exception? exception = null)
    {
        if (!IsConnected || isClosing) { return; }
        isClosing = true;
        cancellationTokenSource.Cancel();
        try
        {
            networkStream?.Write(BitConverter.GetBytes(-1), 0, 4);
        }
        catch { }
        networkStream?.Dispose();
        IsConnected = false;
        isClosing = false;
        if (exception is not null)
        {
            ErrorOccurred?.Invoke(this, new(0, reason, exception));
        }
    }

    /// <summary>
    /// Send data to the connected server/client
    /// </summary>
    /// <param name="data">The memory of bytes to send.</param>
    public async Task SendAsync(byte[] data)
    {
        if (!IsConnected || networkStream is null) { throw new InvalidOperationException("Sending data requires a open connection."); }
        try
        {

            await networkStream.WriteAsync(BitConverter.GetBytes(data.Length));
            await networkStream.WriteAsync(data);
            return;
        }
        catch (Exception e)
        {
            Disconnect("Error sending data", e);
        }
        throw new InvalidOperationException("Data could not be sent. An existing connection is required.");
    }

    public void Dispose()
    {
        Disconnect();
        tcpClient.Dispose();
        GC.SuppressFinalize(this);
    }

}
