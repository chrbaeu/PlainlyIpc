using PlainlyIpc.Tcp;
using System.Net;
using System.Net.Sockets;

namespace PlainlyIpcTests.Tcp;

public class TcpTest
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();
    private TaskCompletionSource<bool> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Fact]
    public async Task SendAndReceiveData()
    {
        using ManagedTcpListener server = new(ipEndPoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAsync());

        using ManagedTcpClient client = new(ipEndPoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };

        await Task.Delay(10);
        await client.ConnectAsync();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));

        passed.Should().BeTrue();
    }

    [Fact]
    public void ServerPortInUseTest()
    {
        using ManagedTcpListener server1 = new(ipEndPoint);
        _ = server1.StartListenAsync();
        Assert.Throws<SocketException>(() =>
        {
            using ManagedTcpListener server2 = new(ipEndPoint);
            _ = server2.StartListenAsync();
        });
    }

    [Fact]
    public async Task ConnectAndReconnectTest()
    {
        using ManagedTcpListener server = new(ipEndPoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAsync());

        ManagedTcpClient client = new(ipEndPoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
        client.Dispose();

        tsc = new();

        client = new(ipEndPoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();

        passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
        client.Dispose();
    }

    [Fact]
    public async Task ServerFailedTest()
    {
        using ManagedTcpListener server = new(ipEndPoint);
        server.IncomingTcpClient += (object? sender, IncomingTcpClientEventArgs e) =>
        {
            e.TcpClient.Dispose();
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = server.StartListenAsync();

        using ManagedTcpClient client = new(ipEndPoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Equal(ErrorEventCode.ConnectionLost, e.ErrorCode);
        };
        await client.ConnectAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        });
    }

    [Fact]
    public async Task NoServerTest()
    {
        using ManagedTcpClient client = new(ipEndPoint);

        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await client.ConnectAsync(250);
        });
    }

    [Fact]
    public async Task StartAndStopSendAndReciveData()
    {
        using ManagedTcpListener server = new(ipEndPoint);

        var serverTask = server.StartListenAsync();
        server.Stop();
        await Task.Delay(10);
        Assert.True(serverTask.IsCompleted);

        server.IncomingTcpClient += async (sender, e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        };
        server.ErrorOccurred += (sender, e) =>
        {
            Assert.Fail(e.Message);
        };
        _ = server.StartListenAsync();
        Assert.True(server.IsListening);

        using ManagedTcpClient client = new(ipEndPoint);
        client.DataReceived += (sender, e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (sender, e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));

        passed.Should().BeTrue();
    }

}
