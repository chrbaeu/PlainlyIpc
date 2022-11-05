using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace PlainlyIpc.NamedPipe;

/// <summary>
/// Named pipe server implementing IDataHandler.
/// </summary>
internal sealed class NamedPipeServer : IDataHandler, IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private NamedPipeServerStream server;
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
    /// Indicates if the named pipe server listens for incoming data. 
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indicates if the named pipe server is connected to a client.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Creates a named pipe server for the given named pipe name.
    /// </summary>
    /// <param name="namedPipeName">Name of the named pipe.</param>
    public NamedPipeServer(string namedPipeName)
    {
        NamedPipeName = namedPipeName;
        server = new(namedPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }

    /// <summary>
    /// Starts the named pipe server instance.
    /// </summary>
    public Task StartAsync()
    {
        if (IsActive) { return Task.CompletedTask; }
        IsActive = true;
        return Task.Run(() => WaitForClient());
    }

    /// <inheritdoc/>
    public async Task SendAsync(byte[] data)
    {
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        if (!IsActive) { throw new InvalidOperationException($"{nameof(NamedPipeServer)} must be active to send data!"); }
        await server.WriteAsync(BitConverter.GetBytes(data.Length)).ConfigureAwait(false);
        await server.WriteAsync(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed) { return; }
        isDisposed = true;
        IsActive = false;
        IsConnected = false;
        cancellationTokenSource.Cancel();
        server.Dispose();
        cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task WaitForClient()
    {
        try
        {
            while (IsActive)
            {
                await server.WaitForConnectionAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                IsConnected = true;
                await AcceptIncommingData().ConfigureAwait(false);
                if (IsActive)
                {
#if NETSTANDARD2_0
                    server.Dispose();
#else
                    await server.DisposeAsync().ConfigureAwait(false);
#endif
                    IsConnected = false;
                    server = new(NamedPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new(0, "A connection based error has occurred. The named pipe server is stopped.", e));
        }
        IsActive = false;
    }

    private async Task AcceptIncommingData()
    {
        try
        {
            byte[] lenArray = new byte[4];
            while (IsActive)
            {
                try
                {
                    await server.ReadExactly(lenArray, 4, cancellationTokenSource.Token).ConfigureAwait(false);
                }
                catch (EndOfStreamException e)
                {
                    ErrorOccurred?.Invoke(this, new(IpcErrorCode.ConnectionLost, "The connection to the client was lost.", e));
                    break;
                }
                var dataLen = BitConverter.ToInt32(lenArray, 0);
                if (dataLen == -1) { break; }
                byte[] dataArray = new byte[dataLen];
                DataReceivedEventArgs dataReceivedEventArgs = new(dataArray);
                await server.ReadExactly(dataArray, dataLen, cancellationTokenSource.Token).ConfigureAwait(false);
                DataReceived?.Invoke(this, dataReceivedEventArgs);
            }
        }
        catch (OperationCanceledException) { /* The server is stopped */  }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new(IpcErrorCode.UnexpectedError, "An error occurred while reading the received data.", e));
        }
    }

}
