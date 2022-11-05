namespace PlainlyIpcTests.Rpc.PlainlyIpcProxies;

public class RpcTestServiceRemoteProxy : PlainlyIpcTests.Rpc.IRpcTestService
{
    private readonly IIpcHandler ipcHandler;

    public RpcTestServiceRemoteProxy(IIpcHandler ipcHandler)
    {
        this.ipcHandler = ipcHandler;
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public Int32 Add(Int32 a, Int32 b)
    {
        return Task.Run(() => AddAsync(a, b)).GetAwaiter().GetResult();
    }

    public async Task<Int32> AddAsync(Int32 a, Int32 b)
    {
        return await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService, Int32>(plainlyRpcService
            => plainlyRpcService.Add(a, b));
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public void NoResultOp(Int32 x)
    {
        Task.Run(() => NoResultOpAsync(x)).GetAwaiter().GetResult();
    }

    public async Task NoResultOpAsync(Int32 x)
    {
        await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService>(plainlyRpcService
            => plainlyRpcService.NoResultOp(x));
    }

    public async Task<System.Int32> Sum(System.Collections.Generic.IEnumerable<System.Int32> values)
    {
        return await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService, Int32>(plainlyRpcService
            => plainlyRpcService.Sum(values));
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public System.Collections.Generic.IEnumerable<System.Int32> Convert(params Int32[] values)
    {
        return Task.Run(() => ConvertAsync(values)).GetAwaiter().GetResult();
    }

    public async Task<System.Collections.Generic.IEnumerable<System.Int32>> ConvertAsync(params Int32[] values)
    {
        return await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService, System.Collections.Generic.IEnumerable<System.Int32>>(plainlyRpcService
            => plainlyRpcService.Convert(values));
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public T Generic<T>(T value)
    {
        return Task.Run(() => GenericAsync(value)).GetAwaiter().GetResult();
    }

    public async Task<T> GenericAsync<T>(T value)
    {
        return await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService, T>(plainlyRpcService
            => plainlyRpcService.Generic(value));
    }

    public async Task GetTask()
    {
        await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService, Task>(plainlyRpcService
            => plainlyRpcService.GetTask());
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public Int32 ThrowError(String test)
    {
        return Task.Run(() => ThrowErrorAsync(test)).GetAwaiter().GetResult();
    }

    public async Task<Int32> ThrowErrorAsync(String test)
    {
        return await ipcHandler.ExecuteRemote<PlainlyIpcTests.Rpc.IRpcTestService, Int32>(plainlyRpcService
            => plainlyRpcService.ThrowError(test));
    }

}

