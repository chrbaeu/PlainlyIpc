using PlainlyIpc.Tcp;

namespace PlainlyIpc.EventArgs;

/// <summary>
/// Event class for connecting TCP clients.
/// </summary>
internal sealed class IncomingTcpClientEventArgs : System.EventArgs
{
    /// <summary>
    /// The connecting TCP client.
    /// </summary>
    public ManagedTcpClient TcpClient { get; }


    /// <summary>
    /// Creates a new event for a connecting TCP client.
    /// </summary>
    /// <param name="tcpClient">The connecting TCP client.</param>
    public IncomingTcpClientEventArgs(ManagedTcpClient tcpClient)
    {
        TcpClient = tcpClient;
    }

}
