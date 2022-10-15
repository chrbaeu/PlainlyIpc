using PlainlyIpc.Converter;
using PlainlyIpc.Interfaces;
using PlainlyIpc.Internal;
using PlainlyIpc.NamedPipe;
using System.IO;
using System.Threading.Tasks;

namespace PlainlyIpc;

public class NamedPipeIpcClient : INamedPipeIpcClient
{
    private readonly NamedPipeClient namedPipeClient;
    private readonly IObjectConverter objectConverter;

    public NamedPipeIpcClient(string namedPipeName, IObjectConverter? objectConverter = null)
    {
        this.namedPipeClient = new(namedPipeName);
        this.objectConverter = objectConverter ?? new BinaryObjectConverter();
    }

    public NamedPipeIpcClient(NamedPipeClient namedPipeClient, IObjectConverter objectConverter)
    {
        this.namedPipeClient = namedPipeClient;
        this.objectConverter = objectConverter;
    }

    public void Connect() => namedPipeClient.Connect();

    public Task ConnectAsync() => namedPipeClient.ConnectAsync();

    public void Send(string data)
    {
        using MemoryStream memoryStream = new();
        memoryStream.WriteUtf8String(typeof(string).GetTypeString());
        memoryStream.WriteUtf8String(data);
        namedPipeClient.Send(memoryStream.ToArray());
    }

    public void Send<T>(T data)
    {
        using MemoryStream memoryStream = new();
        memoryStream.WriteUtf8String(typeof(T).GetTypeString());
        memoryStream.WriteArray(objectConverter.Serialize<T>(data));
        namedPipeClient.Send(memoryStream.ToArray());
    }

    public async Task SendAsync(string data)
    {
        using MemoryStream memoryStream = new();
        memoryStream.WriteUtf8String(typeof(string).GetTypeString());
        memoryStream.WriteUtf8String(data);
        await namedPipeClient.SendAsync(memoryStream.ToArray());
    }

    public async Task SendAsync<T>(T data)
    {
        using MemoryStream memoryStream = new();
        memoryStream.WriteUtf8String(typeof(T).GetTypeString());
        memoryStream.WriteArray(objectConverter.Serialize<T>(data));
        await namedPipeClient.SendAsync(memoryStream.ToArray());
    }

}
