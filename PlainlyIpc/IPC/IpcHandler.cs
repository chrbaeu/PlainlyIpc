using PlainlyIpc.Rpc;
using System.Diagnostics;
using System.Linq.Expressions;

namespace PlainlyIpc.IPC;

/// <summary>
/// IPC Handler class
/// </summary>
public sealed class IpcHandler : IIpcHandler
{
    private readonly Dictionary<Guid, TaskCompletionSource<RemoteResult>> tcsDict = new();
    private readonly Dictionary<Type, object> serviceDict = new();
    private readonly IpcSender ipcSender;
    private readonly IpcReceiver ipcReceiver;
    private readonly IObjectConverter objectConverter;
    private bool isDisposed;

    /// <summary>
    /// Timeout for remote calls.
    /// </summary>
    public TimeSpan RemoteTimeout { get; set; } = new TimeSpan(0, 0, 10);

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
    public async Task SendAsync(byte[] data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        await ipcSender.SendAsync(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        await ipcSender.SendAsync(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SendStringAsync(string data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        await ipcSender.SendStringAsync(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SendObjectAsync<T>(T data)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (data is null) { throw new ArgumentNullException(nameof(data)); }
        await ipcSender.SendObjectAsync<T>(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void RegisterService(Type type, object service)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (type is null) { throw new ArgumentNullException(nameof(type)); }
        if (service is null) { throw new ArgumentNullException(nameof(service)); }
        serviceDict.Add(type, service);
    }

    /// <inheritdoc/>
    public void RegisterService<TIService>(TIService service) where TIService : notnull
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (service is null) { throw new ArgumentNullException(nameof(service)); }
        serviceDict.Add(typeof(TIService), service);
    }

    /// <inheritdoc/>
    public async Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, Task<TResult>>> func)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (func is null) { throw new ArgumentNullException(nameof(func)); }
        return await ExecuteRemote<TIRemnoteService, TResult>((Expression)func).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, TResult>> func)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (func is null) { throw new ArgumentNullException(nameof(func)); }
        return await ExecuteRemote<TIRemnoteService, TResult>((Expression)func).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ExecuteRemote<TIRemnoteService>(Expression<Action<TIRemnoteService>> func)
    {
        if (isDisposed) { throw new ObjectDisposedException(nameof(IpcHandler)); }
        if (func is null) { throw new ArgumentNullException(nameof(func)); }
        await ExecuteRemote<TIRemnoteService, object>((Expression)func).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed) { return; }
        isDisposed = true;
        foreach (var item in tcsDict.ToList())
        {
            tcsDict.Remove(item.Key);
            item.Value?.TrySetException(new ObjectDisposedException($"RemoteExecuter is disposed!"));
        }
        ipcSender.Dispose();
        ipcReceiver.MessageReceived -= Receiver_MessageReceived;
        ipcReceiver.ErrorOccurred -= Receiver_ErrorOccurred;
        ipcReceiver.Dispose();
        MessageReceived = null;
        ErrorOccurred = null;
        GC.SuppressFinalize(this);
    }

    private async Task<TResult> ExecuteRemote<TIRemoteService, TResult>(Expression func)
    {
        Debug.WriteLine("#>ExecuteRemote");
        RemoteCall remoteCall = RemoteMessageHelper.FromCall(typeof(TIRemoteService), func, objectConverter);
        TaskCompletionSource<RemoteResult> resultTcs = new();
        tcsDict.Add(remoteCall.Uuid, resultTcs);
        Debug.WriteLine("#>ExecuteRemote>SendCall");
        await ipcSender.SendRemoteMessageAsync(remoteCall.AsBytes()).ConfigureAwait(false);
        Debug.WriteLine("#>ExecuteRemote>Call sended");
        try
        {
            Debug.WriteLine("#>ExecuteRemote>Wait for result");
            await resultTcs.Task.WaitAsync(RemoteTimeout).ConfigureAwait(false);
            Debug.WriteLine("#>ExecuteRemote>Got result");
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
        if (resultTcs.Task.IsCompleted)
        {
            Debug.WriteLine("#>ExecuteRemote>Deserialize");
            return objectConverter.Deserialize<TResult>(resultTcs.Task.GetAwaiter().GetResult().ResultData)!;
        }
        throw new RemoteException("Unexpected error");
    }

    private async void Receiver_MessageReceived(object? sender, IpcMessageReceivedEventArgs e)
    {
        Debug.WriteLine("#>Receiver_MessageReceived");
        if (e.MsgType != IpcMessageType.RemoteMessage)
        {
            MessageReceived?.Invoke(this, e);
            return;
        }
        var remoteAction = RemoteMessageHelper.FromBytes(((Memory<byte>)e.Value!).ToArray());
        switch (remoteAction)
        {
            case RemoteCall remoteCall:
                if (serviceDict.TryGetValue(remoteCall.InterfaceType, out var serviceInstance))
                {
                    RemoteMessage result = await RemoteCallExecuter.Execute(remoteCall, serviceInstance, objectConverter).ConfigureAwait(false);
                    await ipcSender.SendRemoteMessageAsync(result.AsBytes()).ConfigureAwait(false);
                }
                break;
            case RemoteResult remoteResult:
                if (tcsDict.TryGetValue(remoteResult.Uuid, out var resultTcs))
                {
                    resultTcs.SetResult(remoteResult);
                    tcsDict.Remove(remoteResult.Uuid);
                }
                break;
            case RemoteError remoteError:
                if (tcsDict.TryGetValue(remoteError.Uuid, out var errorTcs))
                {
                    errorTcs.SetException(new RemoteException(remoteError.ErrorMessage));
                    tcsDict.Remove(remoteError.Uuid);
                }
                break;
        }
    }

    private void Receiver_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

}
