using PlainlyIpcTests.Rpc.Services;

namespace PlainlyIpcTests.Rpc;

public class ChatServiceNpTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private readonly IpcFactory ipcFactory = new();
    private readonly TaskCompletionSource<bool> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Test]
    public async Task SendMessageTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.RegisterService<IChatService>(new ChatService(async x =>
        {
            await Assert.That(x).IsEqualTo(TestData.Text);
            tsc.SetResult(true);
        }));

        using IIpcHandler handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);
        await handlerC.ExecuteRemote<IChatService>(x => x.SendMessage(TestData.Text));

        var passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(passed).IsTrue();
    }
}
