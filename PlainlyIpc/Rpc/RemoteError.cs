namespace PlainlyIpc.Rpc;

internal sealed class RemoteError : RemoteMessage
{
    public RemoteError(Guid uuid, string errorMessage) : base(RemoteMsgType.RemoteError, uuid)
    {
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// The error message if something goes wrong.
    /// </summary>
    public string ErrorMessage { get; }

}
