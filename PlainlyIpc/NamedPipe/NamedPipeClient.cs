using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace PlainlyIpc.NamedPipe;

/// <summary>
/// Named pipe client implementing IDataHandler.
/// </summary>
internal sealed class NamedPipeClient : IDataHandler, IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly NamedPipeClientStream client;
    private bool isDisposed;

    /// <inheritdoc/>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <inheritdoc/>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <summary>
    /// The name of the named pipe.
    /// </summary>
    public string NamedPipeName { get; }

    /// <summary>
    /// Indicates if the named pipe client is connected to a server.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Creates a named pipe client for the given named pipe name.
    /// </summary>
    /// <param name="namedPipeName">Name of the named pipe.</param>
    public NamedPipeClient(string namedPipeName)
    {
        NamedPipeName = namedPipeName;
        client = new(".", namedPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    }

    /// <summary>
    /// Connects to the named pipe.
    /// </summary>
    /// <param name="connectionTimeout">Timeout in ms.</param>
    /// <returns>Task to await the connection to be established.</returns>
    /// <exception cref="IOException"></exception>
    public async Task ConnectAsync(int connectionTimeout = 5000)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(NamedPipeClient)); }
        if (IsConnected) { return; }
        if (connectionTimeout <= 0)
        {
            await client.ConnectAsync().ConfigureAwait(false);
        }
        else
        {
            await client.ConnectAsync(connectionTimeout).ConfigureAwait(false);
        }
        if (!client.IsConnected)
        {
            throw new IOException($"Connecting to named pipe '{NamedPipeName}' failed!");
        }
        IsConnected = true;
        _ = AcceptIncommingData();
    }

    /// <inheritdoc/>
    public async Task SendAsync(byte[] data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(NamedPipeClient)); }
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (!IsConnected) { throw new InvalidOperationException($"{nameof(NamedPipeClient)} must be connected to send data!"); }
        try
        {
            await client.WriteAsync(BitConverter.GetBytes(data.Length)).ConfigureAwait(false);
            await client.WriteAsync(data).ConfigureAwait(false);
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
        client.Dispose();
        cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task AcceptIncommingData()
    {
        try
        {
            byte[] lenArray = new byte[4];
            while (IsConnected && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await client.ReadExactly(lenArray, 4, cancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch (EndOfStreamException e)
                {
                    IsConnected = false;
                    ErrorOccurred?.Invoke(this, new(ErrorEventCode.ConnectionLost, "The connection was lost.", e));
                    break;
                }
                int dataLen = BitConverter.ToInt32(lenArray, 0);
                byte[] dataArray = new byte[dataLen];
                await client.ReadExactly(dataArray, dataLen, cancellationTokenSource.Token).ConfigureAwait(false);
                DataReceived?.Invoke(this, new(dataArray));
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
