using PlainlyIpc.Tcp;
using System.Net;
using System.Net.Sockets;

namespace PlainlyIpcTests.NamedPipe;

public class TcpTest
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();

    [Fact]
    public async Task SendAndReciveData()
    {
        TaskCompletionSource<bool> tsc = new();

        ManagedTcpListener server = new(ipEndPoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

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
        await client.ConnectAsync();
        _ = client.AcceptIncommingData();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));

        passed.Should().BeTrue();
    }

    [Fact]
    public void ServerPortInUseTest()
    {
        ManagedTcpListener server1 = new(ipEndPoint);
        _ = server1.StartListenAync();
        Assert.Throws<SocketException>(() =>
        {
            ManagedTcpListener server2 = new(ipEndPoint);
            _ = server2.StartListenAync();
        });
    }

    [Fact]
    public async Task ConnectAndReconnectTest()
    {
        TaskCompletionSource<bool> tsc = new();

        ManagedTcpListener server = new(ipEndPoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

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
        _ = client.AcceptIncommingData();

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
        _ = client.AcceptIncommingData();

        passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));
        passed.Should().BeTrue();
        client.Dispose();
    }

    [Fact]
    public async Task ServerFailedTest()
    {
        TaskCompletionSource<bool> tsc = new();

        ManagedTcpListener server = new(ipEndPoint);
        server.IncomingTcpClient += (object? sender, IncomingTcpClientEventArgs e) =>
        {
            e.TcpClient.Dispose();
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

        using ManagedTcpClient client = new(ipEndPoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();
        _ = client.AcceptIncommingData();

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

}
