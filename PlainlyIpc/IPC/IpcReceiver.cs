using System.IO;

namespace PlainlyIpc.IPC;

/// <summary>
/// IPC Receiver class.
/// </summary>
public sealed class IpcReceiver : IIpcReceiver, IDisposable
{
    private readonly IDataReceiver dataReceiver;
    private readonly IObjectConverter objectConverter;
    private bool isDisposed;

    /// <inheritdoc/>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
    /// <inheritdoc/>
    public event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Creates a new IPC receiver based on the given data receiver and object converter.
    /// </summary>
    /// <param name="dataReceiver">The data receiver.</param>
    /// <param name="objectConverter">The object converter.</param>
    public IpcReceiver(IDataReceiver dataReceiver, IObjectConverter objectConverter)
    {
        this.dataReceiver = dataReceiver;
        this.objectConverter = objectConverter;
        this.dataReceiver.DataReceived += DataReceiver_MessageReceived;
        this.dataReceiver.ErrorOccurred += DataReceiver_ErrorOccurred;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed) { return; }
        isDisposed = true;
        dataReceiver.DataReceived -= DataReceiver_MessageReceived;
        dataReceiver.ErrorOccurred -= DataReceiver_ErrorOccurred;
        dataReceiver.Dispose();
        MessageReceived = null;
        ErrorOccurred = null;
        GC.SuppressFinalize(this);
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
                data = Encoding.UTF8.GetString(args.Data, 1, args.Data.Length - 2);
            }
            else if (msgType == IpcMessageType.ObjectData)
            {
                using MemoryStream memoryStream = new(args.Data, 1, args.Data.Length - 1);
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
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(0, "Processing of received data failed.", e));
            return;
        }
        MessageReceived?.Invoke(this, new IpcMessageReceivedEventArgs(msgType, data, type));
    }

    private void DataReceiver_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

}
