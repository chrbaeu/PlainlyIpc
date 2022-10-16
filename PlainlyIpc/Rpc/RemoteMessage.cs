namespace PlainlyIpc.Rpc;

internal abstract class RemoteMessage
{
    public RemoteMessage(RemoteMsgType msgType, Guid uuid) => (MsgType, Uuid) = (msgType, uuid);

    /// <summary>
    /// The type of the remote call.
    /// </summary>
    public RemoteMsgType MsgType { get; }

    /// <summary>
    /// The UUID of the call itself.
    /// </summary>
    public Guid Uuid { get; }

}
