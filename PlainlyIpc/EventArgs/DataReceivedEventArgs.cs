namespace PlainlyIpc.EventArgs;

public sealed class DataReceivedEventArgs : System.EventArgs
{

    public byte[] Data { get; set; }

    public DataReceivedEventArgs(byte[] data)
    {
        Data = data;
    }

}
