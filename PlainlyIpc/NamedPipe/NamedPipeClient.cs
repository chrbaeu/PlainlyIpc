using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace PlainlyIpc.NamedPipe;

/// <summary>
/// Named pipe client implementing IDataHandler.
/// </summary>
public sealed class NamedPipeClient : IDataHandler, IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly NamedPipeClientStream client;

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
    public async Task ConnectAsync(int connectionTimeout = 1000)
    {
        if (IsActive) { return; }
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
        IsActive = true;
        _ = AcceptIncommingData();
    }

    /// <inheritdoc/>
    public async Task SendAsync(byte[] data)
    {
        if (!IsActive) { throw new InvalidOperationException($"{nameof(NamedPipeClient)} must be active to send data!"); }
        await client.WriteAsync(BitConverter.GetBytes(data.Length)).ConfigureAwait(false);
        await client.WriteAsync(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        IsActive = false;
        cancellationTokenSource.Cancel();
        if (client.IsConnected)
        {
            try
            {
                client.Write(BitConverter.GetBytes(-1), 0, 4);
                client.Flush();
            }
            catch { }
        }
        client.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task AcceptIncommingData()
    {
        try
        {
            byte[] lenArray = new byte[4];
            while (IsActive)
            {
                await client.ReadExactly(lenArray, 4, cancellationTokenSource.Token).ConfigureAwait(false);
                var dataLen = BitConverter.ToInt32(lenArray, 0);
                if (dataLen == -1) { break; }
                byte[] dataArray = new byte[dataLen];
                DataReceivedEventArgs dataReceivedEventArgs = new(dataArray);
                await client.ReadExactly(dataArray, dataLen, cancellationTokenSource.Token).ConfigureAwait(false);
                DataReceived?.Invoke(this, dataReceivedEventArgs);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new(0, "An error occurred while reading the received data.", e));
        }
        IsActive = false;
    }

}
