namespace PlainlyIpcChatDemo.RpcDemo.Services;

internal sealed class ChatService : IChatService
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
