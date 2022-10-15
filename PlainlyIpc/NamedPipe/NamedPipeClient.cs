using PlainlyIpc.Interfaces;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace PlainlyIpc.NamedPipe;

public class NamedPipeClient : IDataSender, IDisposable
{
    private readonly NamedPipeClientStream client;

    public string NamedPipeName { get; }
    public bool IsConnected => client.IsConnected;

    public NamedPipeClient(string namedPipeName)
    {
        NamedPipeName = namedPipeName;
        client = new(namedPipeName);
    }

    public void Connect(int connectionTimeout = 1000)
    {
        if (IsConnected) { return; }
        if (connectionTimeout <= 0)
        {
            client.Connect();
        }
        else
        {
            client.Connect(connectionTimeout);
        }
        if (!IsConnected)
        {
            throw new IOException($"Connecting to named pipe '{NamedPipeName}' failed!");
        }
    }

    public async Task ConnectAsync(int connectionTimeout = 1000)
    {
        if (IsConnected) { return; }
        if (connectionTimeout <= 0)
        {
            await client.ConnectAsync();
        }
        else
        {
            await client.ConnectAsync(connectionTimeout);
        }
        if (!IsConnected)
        {
            throw new IOException($"Connecting to named pipe '{NamedPipeName}' failed!");
        }
    }

    public void Send(byte[] data)
    {
        if (!IsConnected) { throw new InvalidOperationException($"{nameof(NamedPipeClient)} must be connected to send data!"); }
        client.Write(BitConverter.GetBytes(data.Length), 0, 4);
        client.Write(data, 0, data.Length);
    }

    public async Task SendAsync(byte[] data)
    {
        if (!IsConnected) { throw new InvalidOperationException($"{nameof(NamedPipeClient)} must be connected to send data!"); }
#if NET6_0_OR_GREATER
        await client.WriteAsync(BitConverter.GetBytes(data.Length));
        await client.WriteAsync(data);
#else
        await client.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
        await client.WriteAsync(data, 0, data.Length);
#endif
    }

    public void Dispose()
    {
        if (client.IsConnected)
        {
            client.Write(BitConverter.GetBytes(-1), 0, 4);
            client.Flush();
        }
        client.Dispose();
        GC.SuppressFinalize(this);
    }

}
