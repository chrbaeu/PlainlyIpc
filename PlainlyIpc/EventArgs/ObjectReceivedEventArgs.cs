using System;

namespace PlainlyIpc.EventArgs;

public class ObjectReceivedEventArgs : System.EventArgs
{
    public Type Type { get; }
    public object? Value { get; }

    public ObjectReceivedEventArgs(Type type, object? value)
    {
        this.Type = type;
        this.Value = value;
    }

}
