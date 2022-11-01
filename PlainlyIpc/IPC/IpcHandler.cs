using PlainlyIpc.Rpc;
using System.Linq.Expressions;

namespace PlainlyIpc.IPC;

/// <summary>
/// IPC Handler class
/// </summary>
public class IpcHandler : IIpcHandler
{
    private readonly Dictionary<Guid, TaskCompletionSource<RemoteResult>> tcsDict = new();
    private readonly Dictionary<Type, object> serviceDict = new();
    private readonly IpcSender ipcSender;
    private readonly IpcReceiver ipcReceiver;
    private readonly IObjectConverter objectConverter;

    /// <summary>
    /// Timeout for remote calls.
    /// </summary>
    public TimeSpan RemoteTimeout { get; set; } = new TimeSpan(0, 0, 5);

    /// <inheritdoc/>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <inheritdoc/>
    public event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Creates a new IPC handler from a data handler.
    /// </summary>
    /// <param name="dataHandler">The IDataHandler instance.</param>
    /// <param name="objectConverter">The IObjectConverter instance.</param>
    public IpcHandler(IDataHandler dataHandler, IObjectConverter objectConverter)
        : this(dataHandler, dataHandler, objectConverter)
    {
    }

    /// <summary>
    /// Creates a new IPC handler from a data sender and receiver.
    /// </summary>
    /// <param name="dataSender">The IDataSender instance.</param>
    /// <param name="dataReceiver">The IDataReceiver instance.</param>
    /// <param name="objectConverter">The IObjectConverter instance.</param>
    public IpcHandler(IDataSender dataSender, IDataReceiver dataReceiver, IObjectConverter objectConverter)
    {
        this.ipcSender = new IpcSender(dataSender, objectConverter);
        this.ipcReceiver = new IpcReceiver(dataReceiver, objectConverter);
        this.objectConverter = objectConverter;
        ipcReceiver.MessageReceived += Receiver_MessageReceived;
        ipcReceiver.ErrorOccurred += Receiver_ErrorOccurred;
    }

    /// <inheritdoc/>
    public Task SendAsync(byte[] data) => ipcSender.SendAsync(data);

    /// <inheritdoc/>
    public Task SendAsync(ReadOnlyMemory<byte> data) => ipcSender.SendAsync(data);

    /// <inheritdoc/>
    public Task SendStringAsync(string data) => ipcSender.SendStringAsync(data);

    /// <inheritdoc/>
    public Task SendObjectAsync<T>(T data) => ipcSender.SendObjectAsync<T>(data);

    /// <inheritdoc/>
    public void RegisterService(Type type, object service)
    {
        serviceDict.Add(type, service);
    }

    /// <inheritdoc/>
    public void RegisterService<TIService>(TIService service) where TIService : notnull
    {
        serviceDict.Add(typeof(TIService), service);
    }

    /// <inheritdoc/>
    public Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, Task<TResult>>> func)
    {
        return ExecuteRemote<TIRemnoteService, TResult>((Expression)func);
    }

    /// <inheritdoc/>
    public Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, TResult>> func)
    {
        return ExecuteRemote<TIRemnoteService, TResult>((Expression)func);
    }

    /// <inheritdoc/>
    public Task ExecuteRemote<TIRemnoteService>(Expression<Action<TIRemnoteService>> func)
    {
        return ExecuteRemote<TIRemnoteService, object>((Expression)func);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var item in tcsDict.ToList())
        {
            tcsDict.Remove(item.Key);
            item.Value.SetException(new ObjectDisposedException($"RemoteExecuter is disposed!"));
        }
        ipcSender.Dispose();
        ipcReceiver.MessageReceived -= Receiver_MessageReceived;
        ipcReceiver.ErrorOccurred -= Receiver_ErrorOccurred;
        ipcReceiver.Dispose();
        MessageReceived = null;
        ErrorOccurred = null;
        GC.SuppressFinalize(this);
    }

    private async Task<TResult> ExecuteRemote<TiRemoteService, TResult>(Expression func)
    {
        RemoteCall remoteCall = RemoteMessageHelper.FromCall(typeof(TiRemoteService), func, objectConverter);
        TaskCompletionSource<RemoteResult> result = new();
        tcsDict.Add(remoteCall.Uuid, result);
        await ipcSender.SendRemoteMessageAsync(remoteCall.AsBytes());
        try
        {
            await result.Task.WaitAsync(RemoteTimeout);
        }
        catch (RemoteException)
        {
            throw;
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RemoteException("Unexpected error", e);
        }
        if (result.Task.IsCompleted)
        {
            return objectConverter.Deserialize<TResult>((await result.Task).Result)!;
        }
        throw new RemoteException("Unexpected error");
    }

    private async void Receiver_MessageReceived(object? sender, IpcMessageReceivedEventArgs e)
    {
        if (e.MsgType != IpcMessageType.RemoteMessage)
        {
            MessageReceived?.Invoke(this, e);
            return;
        }
        var remoteAction = RemoteMessageHelper.FromBytes(((Memory<byte>)e.Value!).ToArray());
        TaskCompletionSource<RemoteResult>? tcs;
        switch (remoteAction)
        {
            case RemoteCall remoteCall:
                if (serviceDict.TryGetValue(remoteCall.InterfaceType, out var serviceInstance))
                {
                    RemoteMessage result = await RemoteCallExecuter.Execute(remoteCall, serviceInstance, objectConverter);
                    await ipcSender.SendRemoteMessageAsync(result.AsBytes());
                }
                break;
            case RemoteResult remoteResult:
                if (tcsDict.TryGetValue(remoteResult.Uuid, out tcs))
                {
                    tcs.SetResult(remoteResult);
                }
                break;
            case RemoteError remoteError:
                if (tcsDict.TryGetValue(remoteError.Uuid, out tcs))
                {
                    tcs.SetException(new RemoteException(remoteError.ErrorMessage));
                }
                break;
        }
    }

    private void Receiver_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

}
