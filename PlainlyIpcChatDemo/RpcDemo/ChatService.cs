namespace PlainlyIpcChatDemo.RpcDemo;

internal class ChatService : IChatService
{
    private readonly Action<string> onMessageAction;

    public ChatService(Action<string> onMessageAction)
    {
        this.onMessageAction = onMessageAction;
    }

    public void SendMessage(string message)
    {
        onMessageAction.Invoke(message);
    }
}
