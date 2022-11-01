using PlainlyIpc.Tcp;

namespace PlainlyIpc.EventArgs;

public sealed class IncomingTcpClientEventArgs : System.EventArgs
{
    public ManagedTcpClient TcpClient { get; }

    public IncomingTcpClientEventArgs(ManagedTcpClient tcpClient)
    {
        TcpClient = tcpClient;
    }

}
