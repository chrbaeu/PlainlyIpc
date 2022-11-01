namespace PlainlyIpc.Rpc;

internal sealed class RemoteResult : RemoteMessage
{
    public RemoteResult(Guid uuid, byte[] resultData) : base(RemoteMsgType.RemoteResult, uuid)
    {
        ResultData = resultData;
    }

    /// <summary>
    /// The reult of the call.
    /// </summary>
    public byte[] ResultData { get; }

}
