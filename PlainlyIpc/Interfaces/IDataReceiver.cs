namespace PlainlyIpc.Interfaces;

public interface IDataReceiver
{
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

}
