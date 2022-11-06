using System.Net;

namespace PlainlyIpcTests.Rpc;

public class TcpChatServiceTest : IAsyncLifetime
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();
    private readonly TaskCompletionSource<bool> tsc = new();
    private IIpcHandler server = null!;
    private IIpcHandler client = null!;

    public async Task InitializeAsync()
    {
        IpcFactory ipcFactory = new();
        server = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        client = await ipcFactory.CreateTcpIpcClient(ipEndPoint);
    }

    public Task DisposeAsync()
    {
        client?.Dispose();
        server?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ChatTest()
    {
        server.RegisterService<IChatService>(new ChatService(async msg =>
        {
            await server.ExecuteRemote<IChatService>(x => x.SendMessage(msg));
        }));
        client.RegisterService<IChatService>(new ChatService(msg =>
        {
            Assert.Equal(TestData.Text, msg);
            tsc.SetResult(true);
        }));

        await client.ExecuteRemote<IChatService>(x => x.SendMessage(TestData.Text));

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
    }

    public interface IChatService
    {
        public void SendMessage(string message);
    }

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

}
