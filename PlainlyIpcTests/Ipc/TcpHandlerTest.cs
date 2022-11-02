using PlainlyIpc.Converter;
using PlainlyIpc.IPC;
using System.Drawing;
using System.Net;

namespace PlainlyIpcTests.Ipc;

public class TcpHandlerTest
{
    private readonly string testText = "Hello World";
    private readonly IpcFactory ipcFactory = new(new JsonObjectConverter());
    private readonly TaskCompletionSource<bool> tsc = new();

    [Fact]
    public async Task TcpIpcHandlerCtoSTest()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Loopback, 60000 + 1);
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler handlerC = await ipcFactory.CreatTcpIpcClient(ipEndPoint);

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
    public async Task TcpPipeIpcHandlerCtoSAndSToCTest()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Loopback, 60000 + 2);
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler handlerC = await ipcFactory.CreatTcpIpcClient(ipEndPoint);

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

        var passed = await tsc.Task.WaitAsync(new TimeSpan(1, 0, 1));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task TcpIpcHandlerReconnectTest()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Loopback, 60000 + 3);
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
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

        IIpcHandler handlerC = await ipcFactory.CreatTcpIpcClient(ipEndPoint);
        await handlerC.SendStringAsync(testText);
        handlerC.Dispose();

        await Task.Delay(100);

        handlerC = await ipcFactory.CreatTcpIpcClient(ipEndPoint);
        await handlerC.SendStringAsync(testText);
        handlerC.Dispose();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task TcpIpcHandlerObjectDataTest()
    {
        IPEndPoint ipEndPoint = new(IPAddress.Loopback, 60000 + 4);
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler handlerC = await ipcFactory.CreatTcpIpcClient(ipEndPoint);

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
