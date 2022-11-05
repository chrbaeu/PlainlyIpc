namespace PlainlyIpc.EventArgs;

/// <summary>
///  Event class for a received IPC message.
/// </summary>
public sealed class IpcMessageReceivedEventArgs : System.EventArgs
{
    /// <summary>
    /// The IPC message type.
    /// </summary>
    public IpcMessageType MsgType { get; }

    /// <summary>
    /// The value / message.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// The type of the value / message.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    ///  Creates a new event for a received IPC message.
    /// </summary>
    /// <param name="msgType">The IPC message type.</param>
    /// <param name="value">The value / message.</param>
    /// <param name="type">The type of the value / message.</param>
    public IpcMessageReceivedEventArgs(IpcMessageType msgType, object? value, Type type)
    {
        MsgType = msgType;
        Type = type;
        Value = value;
    }

}
