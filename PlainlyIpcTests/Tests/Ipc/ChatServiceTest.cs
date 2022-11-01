using PlainlyIpc.Converter;
using PlainlyIpc.IPC;

namespace PlainlyIpcTests.Tests.Ipc;
public class ChatServiceTest
{
    private readonly string testText = "Hello World";
    private readonly IpcFactory ipcFactory = new(new JsonObjectConverter());
    private readonly TaskCompletionSource<bool> tsc = new();

    [Fact]
    public async Task ChatServiceBasicsTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNampedPipeIpcServer(nameof(ChatServiceBasicsTest));
        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.RegisterService<IChatService>(new ChatService(x =>
        {
            x.Should().Be(testText);
            tsc.SetResult(true);
        }));

        using IIpcHandler handlerC = await ipcFactory.CreateNampedPipeIpcClient(nameof(ChatServiceBasicsTest));
        await handlerC.ExecuteRemote<IChatService>(x => x.SendMessage(testText));

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));
        passed.Should().BeTrue();
    }

    public interface IChatService
    {
        public void SendMessage(string message);
    }

    private class ChatService : IChatService
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

}
