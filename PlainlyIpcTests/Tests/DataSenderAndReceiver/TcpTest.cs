using PlainlyIpc.EventArgs;
using PlainlyIpc.Tcp;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlainlyIpcTests.Tests.NamedPipe;

public class TcpTest
{
    private static int Counter;
    private readonly IPEndPoint ipEndpoint = new(IPAddress.Parse("127.0.0.1"), 60042 + Counter++);
    private readonly string testText = "Hello World";

    [Fact]
    public async Task SendAndReciveData()
    {
        TaskCompletionSource<bool> tsc = new();

        MangedTcpListener server = new(ipEndpoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(testText));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

        using ManagedTcpClient client = new(ipEndpoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(testText));
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
        MangedTcpListener server1 = new(ipEndpoint);
        _ = server1.StartListenAync();
        Assert.Throws<SocketException>(() =>
        {
            MangedTcpListener server2 = new(ipEndpoint);
            _ = server2.StartListenAync();
        });
    }

    [Fact]
    public async Task ConnectAndReconnectTest()
    {
        TaskCompletionSource<bool> tsc = new();

        MangedTcpListener server = new(ipEndpoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(testText));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

        ManagedTcpClient client = new(ipEndpoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(testText));
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

        client = new(ipEndpoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(testText));
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

        MangedTcpListener server = new(ipEndpoint);
        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            e.TcpClient.Dispose();
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

        using ManagedTcpClient client = new(ipEndpoint);
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(testText));
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();
        _ = client.AcceptIncommingData();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.SendAsync(Encoding.UTF8.GetBytes(testText));
        });
    }

    [Fact]
    public async Task NoServerTest()
    {
        using ManagedTcpClient client = new(ipEndpoint);

        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await client.ConnectAsync(250);
        });
    }

}
