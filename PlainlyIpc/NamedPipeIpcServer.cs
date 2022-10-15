using PlainlyIpc.Converter;
using PlainlyIpc.EventArgs;
using PlainlyIpc.Interfaces;
using PlainlyIpc.Internal;
using PlainlyIpc.NamedPipe;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PlainlyIpc;

public class NamedPipeIpcServer : IDisposable, INamedPipeIpcServer
{
    private readonly NamedPipeServer namedPipeServer;
    private readonly IObjectConverter objectConverter;

    public event EventHandler<IpcErrorEventArgs>? ErrorOccurred;
    public event EventHandler<ObjectReceivedEventArgs>? ObjectReceived;

    public NamedPipeIpcServer(string namedPipeName, IObjectConverter? objectConverter = null)
        : this(new NamedPipeServer(namedPipeName), objectConverter ?? new BinaryObjectConverter())
    { }

    public NamedPipeIpcServer(NamedPipeServer namedPipeServer, IObjectConverter objectConverter)
    {
        this.namedPipeServer = namedPipeServer;
        this.objectConverter = objectConverter;
        this.namedPipeServer.MessageReceived += NamedPipeServer_MessageReceived;
    }

    public Task StartListenAync() => namedPipeServer.StartListenAync();

    private void NamedPipeServer_MessageReceived(object? sender, DataReceivedEventArgs args)
    {
        using MemoryStream memoryStream = new(args.Data);
        var type = TypeExtensions.GetTypeFromTypeString(memoryStream.ReadUtf8String());
        object? data;
        if (type == typeof(string))
        {
            data = memoryStream.ReadUtf8String();
        }
        else
        {
            data = objectConverter.Deserialize(memoryStream.ReadArray(), type);
        }
        ObjectReceived?.Invoke(this, new ObjectReceivedEventArgs(type, data));
    }

    public void Dispose()
    {
        namedPipeServer.Dispose();
    }

}
