using System.IO;

namespace PlainlyIpc.IPC;

public class IpcReceiver : IIpcReceiver, IDisposable
{
    private readonly IDataReceiver dataReceiver;
    private readonly IObjectConverter objectConverter;

    public event EventHandler<EventArgs.ErrorOccurredEventArgs>? ErrorOccurred;
    public event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;

    public IpcReceiver(IDataReceiver dataReceiver, IObjectConverter objectConverter)
    {
        this.dataReceiver = dataReceiver;
        this.objectConverter = objectConverter;
        this.dataReceiver.DataReceived += DataReceiver_MessageReceived;
        this.dataReceiver.ErrorOccurred += DataReceiver_ErrorOccurred;
    }

    private void DataReceiver_MessageReceived(object? sender, DataReceivedEventArgs args)
    {
        IpcMessageType msgType;
        Type type;
        object? data;
        try
        {
            msgType = (IpcMessageType)args.Data[0];
            if (msgType == IpcMessageType.StringData)
            {
                type = typeof(string);
                data = Encoding.UTF8.GetString(args.Data, 1, args.Data.Length - 1);
            }
            else if (msgType == IpcMessageType.ObjectData)
            {
                using MemoryStream memoryStream = new(args.Data);
                type = TypeExtensions.GetTypeFromTypeString(memoryStream.ReadUtf8String());
                data = objectConverter.Deserialize(memoryStream.ReadArray(), type);
            }
            else
            {
                type = typeof(Memory<byte>);
                data = new Memory<byte>(args.Data, 1, args.Data.Length - 1);
            }
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(this, new EventArgs.ErrorOccurredEventArgs(0, "Processing of received data failed.", e));
            return;
        }
        MessageReceived?.Invoke(this, new IpcMessageReceivedEventArgs(msgType, data, type));
    }

    private void DataReceiver_ErrorOccurred(object? sender, EventArgs.ErrorOccurredEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }


    public void Dispose()
    {
        dataReceiver.DataReceived -= DataReceiver_MessageReceived;
        dataReceiver.ErrorOccurred -= DataReceiver_ErrorOccurred;
        GC.SuppressFinalize(this);
    }

}
