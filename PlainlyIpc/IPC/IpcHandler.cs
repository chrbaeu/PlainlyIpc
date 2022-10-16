using PlainlyIpc.Rpc;
using System.Linq.Expressions;

namespace PlainlyIpc.IPC;

public class IpcHandler : IIpcSender, IIpcReceiver
{
    private readonly Dictionary<Guid, TaskCompletionSource<RemoteResult>> tcsDict = new();
    private readonly Dictionary<Type, object> serviceDict = new();
    private readonly IpcSender ipcSender;
    private readonly IpcReceiver ipcReceiver;
    private readonly IObjectConverter objectConverter;

    public TimeSpan RemoteTimeout { get; set; } = new TimeSpan(0, 0, 5);

    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
    public event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;

    public IpcHandler(IDataSender dataSender, IDataReceiver dataReceiver, IObjectConverter objectConverter)
    {
        this.ipcSender = new IpcSender(dataSender, objectConverter);
        this.ipcReceiver = new IpcReceiver(dataReceiver, objectConverter);
        this.objectConverter = objectConverter;
        ipcReceiver.MessageReceived += Receiver_MessageReceived;
        ipcReceiver.ErrorOccurred += Receiver_ErrorOccurred;
    }

    public IpcHandler(IpcSender ipcSender, IpcReceiver ipcReceiver, IObjectConverter objectConverter)
    {
        this.ipcSender = ipcSender;
        this.ipcReceiver = ipcReceiver;
        this.objectConverter = objectConverter;
        ipcReceiver.MessageReceived += Receiver_MessageReceived;
        ipcReceiver.ErrorOccurred += Receiver_ErrorOccurred;
    }

    public Task SendAsync(byte[] data) => ipcSender.SendAsync(data);

    public Task SendAsync(ReadOnlyMemory<byte> data) => ipcSender.SendAsync(data);

    public Task SendStringAsync(string data) => ipcSender.SendStringAsync(data);

    public Task SendObjectAsync<T>(T data) => ipcSender.SendObjectAsync<T>(data);

    public void RegisterService(Type type, object service)
    {
        serviceDict.Add(type, service);
    }

    public void RegisterService<TIService>(TIService service) where TIService : notnull
    {
        serviceDict.Add(typeof(TIService), service);
    }

    public Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, Task<TResult>>> func)
    {
        return ExecuteRemote<TIRemnoteService, TResult>((Expression)func);
    }

    public Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, TResult>> func)
    {
        return ExecuteRemote<TIRemnoteService, TResult>((Expression)func);
    }

    public Task ExecuteRemote<TIRemnoteService>(Expression<Action<TIRemnoteService>> func)
    {
        return ExecuteRemote<TIRemnoteService, object>((Expression)func);
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

    public void Dispose()
    {
        foreach (var item in tcsDict.ToList())
        {
            tcsDict.Remove(item.Key);
            item.Value.SetException(new ObjectDisposedException($"RemoteExecuter is disposed!"));
        }
        ipcReceiver.MessageReceived -= Receiver_MessageReceived;
        ipcReceiver.ErrorOccurred -= Receiver_ErrorOccurred;
        GC.SuppressFinalize(this);
    }

}
