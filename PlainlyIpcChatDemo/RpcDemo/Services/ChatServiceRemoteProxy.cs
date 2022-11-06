namespace PlainlyIpcChatDemo.RpcDemo.Services;

public class ChatServiceRemoteProxy : IChatService
{
    private readonly IIpcHandler ipcHandler;

    public ChatServiceRemoteProxy(IIpcHandler ipcHandler)
    {
        this.ipcHandler = ipcHandler;
    }

    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
    public void SendMessage(string message)
    {
        Task.Run(() => SendMessageAsync(message)).GetAwaiter().GetResult();
    }

    public async Task SendMessageAsync(string message)
    {
        await ipcHandler.ExecuteRemote<IChatService>(plainlyRpcService
            => plainlyRpcService.SendMessage(message));
    }

}

