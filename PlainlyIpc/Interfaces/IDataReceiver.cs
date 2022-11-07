namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for receiving of data.
/// </summary>
public interface IDataReceiver : IDisposable, IConnectionState
{

    /// <summary>
    /// Event called when data is received.
    /// </summary>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Event called when an error occurs.
    /// </summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

}
