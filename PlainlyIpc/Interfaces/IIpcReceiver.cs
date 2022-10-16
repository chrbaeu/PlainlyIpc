namespace PlainlyIpc.Interfaces;

public interface IIpcReceiver : IDisposable
{
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
    public event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;
}
