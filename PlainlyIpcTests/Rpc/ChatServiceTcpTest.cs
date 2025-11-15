using PlainlyIpcTests.Rpc.Services;
using System.Net;

namespace PlainlyIpcTests.Rpc;

public class ChatServiceTcpTest
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();
    private readonly TaskCompletionSource<string> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Test]
    public async Task ChatTest()
    {
        IpcFactory ipcFactory = new();

        using IIpcHandler server = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler client = await ipcFactory.CreateTcpIpcClient(ipEndPoint);

        server.RegisterService<IChatService>(new ChatService(async msg =>
        {
            await server.ExecuteRemote<IChatService>(x => x.SendMessage(msg));
        }));

        client.RegisterService<IChatService>(new ChatService(tsc.SetResult));

        await client.ExecuteRemote<IChatService>(x => x.SendMessage(TestData.Text));

        var result = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(result).IsEqualTo(TestData.Text);
    }
}

