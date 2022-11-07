namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for the connection state of a connection. 
/// </summary>
public interface IConnectionState
{

    /// <summary>
    /// Indicates if an active connection exists.
    /// May not be up to date if no operation was performed recently.
    /// </summary>
    public bool IsConnected { get; }

}
