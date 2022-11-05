using System.Drawing;
using System.Net;

namespace PlainlyIpcTests.Ipc;

public class TcpHandlerTest
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();
    private readonly IpcFactory ipcFactory = new();
    private readonly TaskCompletionSource<bool> tsc = new();

    [Fact]
    public async Task CtoSTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler handlerC = await ipcFactory.CreateTcpIpcClient(ipEndPoint);

        handlerS.ErrorOccurred += (sender, e) =>
        {
            tsc.TrySetResult(false);
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(TestData.Text);
            tsc.SetResult(true);
        };

        await handlerC.SendStringAsync(TestData.Text);

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task CtoSAndSToCTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler handlerC = await ipcFactory.CreateTcpIpcClient(ipEndPoint);

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
        handlerC.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(TestData.Text + TestData.Text);
            tsc.SetResult(true);
        };

        await handlerC.SendStringAsync(TestData.Text);

        var passed = await tsc.Task.WaitAsync(new TimeSpan(1, 0, 1));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task ReconnectTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        bool state = false;
        handlerS.ErrorOccurred += (sender, e) =>
        {
            if (e.ErrorCode != ErrorEventCode.ConnectionLost) { tsc.TrySetResult(false); }
        };
        handlerS.MessageReceived += (sender, e) =>
        {
            e.Value.Should().Be(TestData.Text);
            if (state)
            {
                tsc.SetResult(true);
            }
            state = true;
        };

        IIpcHandler handlerC = await ipcFactory.CreateTcpIpcClient(ipEndPoint);
        await handlerC.SendStringAsync(TestData.Text);
        handlerC.Dispose();

        await Task.Delay(100);

        handlerC = await ipcFactory.CreateTcpIpcClient(ipEndPoint);
        await handlerC.SendStringAsync(TestData.Text);
        handlerC.Dispose();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task ObjectDataTest()
    {
        using IIpcHandler handlerS = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        using IIpcHandler handlerC = await ipcFactory.CreateTcpIpcClient(ipEndPoint);

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

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
    }

    [Fact]
    public async Task NoClientTest()
    {
        using IIpcHandler server = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        await Assert.ThrowsAsync<IpcException>(async () =>
        {
            await server.SendStringAsync(TestData.Text);
        });
    }

}
