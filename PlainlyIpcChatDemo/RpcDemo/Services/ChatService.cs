namespace PlainlyIpcChatDemo.RpcDemo.Services;

internal class ChatService : IChatService
{
    private readonly Action<string> onMessageAction;

    public ChatService(Action<string> onMessageAction)
    {
        this.onMessageAction = onMessageAction;
    }

    public Task SendMessage(string message)
    {
        onMessageAction.Invoke(message);
        return Task.CompletedTask;
    }
}
