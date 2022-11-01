namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for IPC receiver implementations.
/// </summary>
public interface IIpcReceiver : IDisposable
{

    /// <summary>
    /// Event called when a message is received.
    /// </summary>
    public event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Event called when an error occurs.
    /// </summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

}
