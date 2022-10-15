using PlainlyIpc.EventArgs;
using PlainlyIpc.Internal;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace PlainlyIpc.NamedPipe;

public class NamedPipeServer : IDisposable
{
    private readonly NamedPipeServerStream server;


    public event EventHandler<DataReceivedEventArgs>? MessageReceived;

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
                await server.WaitForConnectionAsync();
                await AcceptIncommingData();
            }
        }
        catch (Exception e)
        {
        }
    }

    public virtual async Task AcceptIncommingData()
    {
        try
        {
            byte[] lenArray = new byte[4];
            while (IsListening)
            {
                await server.ReadExactly(lenArray, 4);
                var dataLen = BitConverter.ToInt32(lenArray, 0);
                if (dataLen == -1) { break; }
                byte[] dataArray = new byte[dataLen];
                DataReceivedEventArgs dataReceivedEventArgs = new(dataArray);
                await server.ReadExactly(dataArray, dataLen);
                MessageReceived?.Invoke(this, dataReceivedEventArgs);
            }
        }
        catch (Exception e)
        {
        }
        IsListening = false;
    }

    public void Dispose()
    {
        IsListening = false;
        server.Dispose();
    }

}
