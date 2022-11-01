namespace PlainlyIpc.EventArgs;

public sealed class IpcMessageReceivedEventArgs : System.EventArgs
{
    public IpcMessageType MsgType { get; }
    public object? Value { get; }
    public Type Type { get; }

    public IpcMessageReceivedEventArgs(IpcMessageType msgType, object? value, Type type)
    {
        this.MsgType = msgType;
        this.Type = type;
        this.Value = value;
    }

}
