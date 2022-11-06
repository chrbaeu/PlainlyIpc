namespace PlainlyIpcTests.Rpc.Services;

public class RpcTestServiceRemoteProxy : IRpcTestService
{
    private readonly IIpcHandler ipcHandler;

    public RpcTestServiceRemoteProxy(IIpcHandler ipcHandler)
    {
        this.ipcHandler = ipcHandler;
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public int Add(int a, int b)
    {
        return Task.Run(() => AddAsync(a, b)).GetAwaiter().GetResult();
    }

    public async Task<int> AddAsync(int a, int b)
    {
        return await ipcHandler.ExecuteRemote<IRpcTestService, int>(plainlyRpcService
            => plainlyRpcService.Add(a, b));
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public void NoResultOp(int x)
    {
        Task.Run(() => NoResultOpAsync(x)).GetAwaiter().GetResult();
    }

    public async Task NoResultOpAsync(int x)
    {
        await ipcHandler.ExecuteRemote<IRpcTestService>(plainlyRpcService
            => plainlyRpcService.NoResultOp(x));
    }

    public async Task<int> Sum(IEnumerable<int> values)
    {
        return await ipcHandler.ExecuteRemote<IRpcTestService, int>(plainlyRpcService
            => plainlyRpcService.Sum(values));
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public IEnumerable<int> Convert(params int[] values)
    {
        return Task.Run(() => ConvertAsync(values)).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<int>> ConvertAsync(params int[] values)
    {
        return await ipcHandler.ExecuteRemote<IRpcTestService, IEnumerable<int>>(plainlyRpcService
            => plainlyRpcService.Convert(values));
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public T Generic<T>(T value)
    {
        return Task.Run(() => GenericAsync(value)).GetAwaiter().GetResult();
    }

    public async Task<T> GenericAsync<T>(T value)
    {
        return await ipcHandler.ExecuteRemote<IRpcTestService, T>(plainlyRpcService
            => plainlyRpcService.Generic(value));
    }

    public async Task GetTask()
    {
        await ipcHandler.ExecuteRemote<IRpcTestService, Task>(plainlyRpcService
            => plainlyRpcService.GetTask());
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public int ThrowError(string test)
    {
        return Task.Run(() => ThrowErrorAsync(test)).GetAwaiter().GetResult();
    }

    public async Task<int> ThrowErrorAsync(string test)
    {
        return await ipcHandler.ExecuteRemote<IRpcTestService, int>(plainlyRpcService
            => plainlyRpcService.ThrowError(test));
    }

}

