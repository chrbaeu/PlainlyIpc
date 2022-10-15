using PlainlyIpc.EventArgs;
using System;
using System.Threading.Tasks;

namespace PlainlyIpc;

public interface INamedPipeIpcServer : IDisposable
{
    public event EventHandler<IpcErrorEventArgs>? ErrorOccurred;
    public event EventHandler<ObjectReceivedEventArgs>? ObjectReceived;

    public Task StartListenAync();
}
