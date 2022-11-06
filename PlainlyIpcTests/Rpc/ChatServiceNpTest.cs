using PlainlyIpcTests.Rpc.Services;

namespace PlainlyIpcTests.Rpc;
public class ChatServiceNpTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private readonly IpcFactory ipcFactory = new();
    private readonly TaskCompletionSource<bool> tsc = new();

    [Fact]
    public async Task SendMessageTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNampedPipeIpcServer(namedPipeName);
        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.RegisterService<IChatService>(new ChatService(x =>
        {
            x.Should().Be(TestData.Text);
            tsc.SetResult(true);
        }));

        using IIpcHandler handlerC = await ipcFactory.CreateNampedPipeIpcClient(namedPipeName);
        await handlerC.ExecuteRemote<IChatService>(x => x.SendMessage(TestData.Text));

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
    }

}
