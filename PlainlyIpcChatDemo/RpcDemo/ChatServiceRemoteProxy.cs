namespace PlainlyIpcChatDemo.RpcDemo.PlainlyIpcProxies;

public class ChatServiceRemoteProxy : PlainlyIpcChatDemo.RpcDemo.IChatService
{
    private readonly IIpcHandler ipcHandler;

    public ChatServiceRemoteProxy(IIpcHandler ipcHandler)
    {
        this.ipcHandler = ipcHandler;
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public void SendMessage(String message)
    {
        Task.Run(() => SendMessageAsync(message)).GetAwaiter().GetResult();
    }

    public async Task SendMessageAsync(String message)
    {
        await ipcHandler.ExecuteRemote<PlainlyIpcChatDemo.RpcDemo.IChatService>(plainlyRpcService
            => plainlyRpcService.SendMessage(message));
    }

}

