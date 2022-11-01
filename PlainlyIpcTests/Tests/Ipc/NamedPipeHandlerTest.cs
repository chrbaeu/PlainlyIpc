using PlainlyIpc.Converter;
using PlainlyIpc.Interfaces;
using PlainlyIpc.IPC;
using System.Drawing;

namespace PlainlyIpcTests.Tests.Ipc;

public class NamedPipeIpcHandlerTest
{
    private readonly string testText = "Hello World";
    private readonly IpcFactory ipcFactory = new(new JsonObjectConverter());
    private readonly TaskCompletionSource<bool> tsc = new();

    [Fact]
    public async Task NamedPipeIpcHandlerCtoSTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNampedPipeIpcServer(nameof(NamedPipeIpcHandlerCtoSTest));
        using IIpcHandler handlerC = await ipcFactory.CreateNampedPipeIpcClient(nameof(NamedPipeIpcHandlerCtoSTest));

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(testText);
            tsc.SetResult(true);
        };

        await handlerC.SendStringAsync(testText);

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task NamedPipeIpcHandlerCtoSAndSToCTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNampedPipeIpcServer(nameof(NamedPipeIpcHandlerCtoSAndSToCTest));
        using IIpcHandler handlerC = await ipcFactory.CreateNampedPipeIpcClient(nameof(NamedPipeIpcHandlerCtoSAndSToCTest));

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            handlerS.SendStringAsync(testText + testText);
        };

        handlerC.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerC.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(testText + testText);
            tsc.SetResult(true);
        };

        await handlerC.SendStringAsync(testText);

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task NamedPipeIpcHandlerReconnectTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNampedPipeIpcServer(nameof(NamedPipeIpcHandlerReconnectTest));
        bool state = false;
        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(testText);
            if (state)
            {
                tsc.SetResult(true);
            }
            state = true;
        };

        IIpcHandler handlerC = await ipcFactory.CreateNampedPipeIpcClient(nameof(NamedPipeIpcHandlerReconnectTest));
        await handlerC.SendStringAsync(testText);
        handlerC.Dispose();

        await Task.Delay(100);

        handlerC = await ipcFactory.CreateNampedPipeIpcClient(nameof(NamedPipeIpcHandlerReconnectTest));
        await handlerC.SendStringAsync(testText);
        handlerC.Dispose();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task NamedPipeIpcHandlerObjectDataTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateNampedPipeIpcServer(nameof(NamedPipeIpcHandlerObjectDataTest));
        using IIpcHandler handlerC = await ipcFactory.CreateNampedPipeIpcClient(nameof(NamedPipeIpcHandlerObjectDataTest));

        var rect = new Rectangle(10, 20, 300, 400);

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(rect);
            tsc.SetResult(true);
        };

        await handlerC.SendObjectAsync(rect);

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));
        passed.Should().BeTrue();
    }

}
