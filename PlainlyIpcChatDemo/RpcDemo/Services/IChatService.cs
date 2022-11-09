namespace PlainlyIpcChatDemo.RpcDemo.Services;

[PlainlyIpc.SourceGenerator.GenerateProxy]
public interface IChatService
{
    public Task SendMessage(string message);
}
