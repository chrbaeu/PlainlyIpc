namespace PlainlyIpc.Rpc;

internal class RemoteResult : RemoteMessage
{
    public RemoteResult(Guid uuid, byte[] result) : base(RemoteMsgType.RemoteResult, uuid)
    {
        Result = result;
    }

    /// <summary>
    /// The reult of the call.
    /// </summary>
    public byte[] Result { get; }

}
