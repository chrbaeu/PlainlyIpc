using System.Drawing;

namespace PlainlyIpcTests.Ipc;

public class NamedPipeIpcHandlerTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private readonly IpcFactory ipcFactory = new();
    private readonly TaskCompletionSource<bool> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<string?> resultTsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Test]
    public async Task CtoSTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        using IIpcHandler handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += async (sender, e) =>
        {
            resultTsc.SetResult(e.Value?.ToString());
        };

        await handlerC.SendStringAsync(TestData.Text);

        var result = await resultTsc.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Assert.That(tsc.Task.IsCompleted).IsFalse();
        await Assert.That(result).IsEqualTo(TestData.Text);
    }

    [Test]
    public async Task CtoSAndSToCTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        using IIpcHandler handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            handlerS.SendStringAsync(TestData.Text + e.Value);
        };

        handlerC.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerC.MessageReceived += async (sender, e) =>
        {
            resultTsc.SetResult(e.Value?.ToString());
        };

        await handlerC.SendStringAsync(TestData.Text);

        var result = await resultTsc.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Assert.That(result).IsEqualTo(TestData.Text + TestData.Text);
    }

    [Test]
    public async Task ReconnectTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        using SemaphoreSlim semaphore = new(0, 1);
        List<string?> receivedMessages = [];
        handlerS.ErrorOccurred += (sender, e) =>
        {
            if (e.ErrorCode != ErrorEventCode.ConnectionLost) { tsc.TrySetResult(false); }
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            receivedMessages.Add(e.Value?.ToString());
            semaphore.Release();
        };

        IIpcHandler handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);
        await Assert.That(await RetryHelper.WaitUntilWithTimeoutAsync(() => handlerS.IsConnected)).IsTrue();
        await Assert.That(await RetryHelper.WaitUntilWithTimeoutAsync(() => handlerC.IsConnected)).IsTrue();
        await handlerC.SendStringAsync(TestData.Text);
        handlerC.Dispose();

        await Assert.That(await semaphore.WaitAsync(TimeSpan.FromSeconds(10))).IsTrue();
        await Assert.That(receivedMessages.Count).IsEqualTo(1);
        await Assert.That(receivedMessages[0]).IsEqualTo(TestData.Text);
        await Assert.That(await RetryHelper.WaitUntilWithTimeoutAsync(() => handlerS.IsConnected, false)).IsFalse();
        await Assert.That(await RetryHelper.WaitUntilWithTimeoutAsync(() => handlerC.IsConnected, false)).IsFalse();

        handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);
        await Assert.That(await RetryHelper.WaitUntilWithTimeoutAsync(() => handlerS.IsConnected)).IsTrue();
        await Assert.That(await RetryHelper.WaitUntilWithTimeoutAsync(() => handlerC.IsConnected)).IsTrue();
        await handlerC.SendStringAsync(TestData.Text);
        handlerC.Dispose();

        await semaphore.WaitAsync(TimeSpan.FromSeconds(10));
        await Assert.That(receivedMessages.Count).IsEqualTo(2);
        await Assert.That(receivedMessages[1]).IsEqualTo(TestData.Text);
    }

    [Test]
    public async Task ObjectDataTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        using IIpcHandler handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);

        var rect = new Rectangle(10, 20, 300, 400);

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += async (sender, e) =>
        {
            await Assert.That(e.Value).IsEqualTo(rect);
            tsc.SetResult(true);
        };

        await handlerC.SendObjectAsync(rect);

        var passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(passed).IsTrue();
    }

    [Test]
    public async Task ObjectDataArrayTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        using IIpcHandler handlerC = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);
        TaskCompletionSource<string[]?> resultTsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

        var data = new[] { "1", "2", "3" };

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += async (sender, e) =>
        {
            resultTsc.SetResult((string[]?)e.Value);
        };

        await handlerC.SendObjectAsync(data);

        var result = await resultTsc.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(result).IsEquivalentTo(data);
    }

    [Test]
    public async Task NoClientTest()
    {
        using IIpcHandler server = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);

        await Assert.That(async () =>
        {
            await server.SendStringAsync(TestData.Text);
        }).Throws<IpcException>();
    }
}
