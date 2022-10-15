using PlainlyIpc.EventArgs;
using PlainlyIpc.Interfaces;
using PlainlyIpc.Internal;
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PlainlyIpc.NamedPipe;

public class NamedPipeServer : IDataReceiver, IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private NamedPipeServerStream server;

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    public string NamedPipeName { get; }
    public bool IsListening { get; private set; }

    public NamedPipeServer(string namedPipeName)
    {
        NamedPipeName = namedPipeName;
        server = new(namedPipeName);
    }

    public Task StartListenAync()
    {
        if (IsListening) { return Task.CompletedTask; }
        IsListening = true;
        return WaitForClient();
    }

    private async Task WaitForClient()
    {
        try
        {
            while (IsListening)
            {
                await server.WaitForConnectionAsync(cancellationTokenSource.Token);
                await AcceptIncommingData();
                if (IsListening)
                {
                    server.Dispose();
                    server = new(NamedPipeName);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new(0, "A connection based error has occurred. The named pipe server is stopped.", e));
        }
        IsListening = false;
    }

    private async Task AcceptIncommingData()
    {
        try
        {
            byte[] lenArray = new byte[4];
            while (IsListening)
            {
                await server.ReadExactly(lenArray, 4, cancellationTokenSource.Token);
                var dataLen = BitConverter.ToInt32(lenArray, 0);
                if (dataLen == -1) { break; }
                byte[] dataArray = new byte[dataLen];
                DataReceivedEventArgs dataReceivedEventArgs = new(dataArray);
                await server.ReadExactly(dataArray, dataLen, cancellationTokenSource.Token);
                DataReceived?.Invoke(this, dataReceivedEventArgs);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new(0, "An error occurred while reading the received data.", e));
        }
    }

    public void Dispose()
    {
        IsListening = false;
        cancellationTokenSource.Cancel();
        server.Dispose();
        GC.SuppressFinalize(this);
    }

}
