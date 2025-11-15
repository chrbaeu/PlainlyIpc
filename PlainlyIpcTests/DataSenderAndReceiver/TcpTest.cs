using PlainlyIpc.Tcp;
using System.Net;
using System.Net.Sockets;

namespace PlainlyIpcTests.Tcp;

public class TcpTest
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();
    private TaskCompletionSource<bool> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Test]
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
        client.DataReceived += async (object? sender, DataReceivedEventArgs e) =>
        {
            await Assert.That(e.Data).IsEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };

        await Task.Delay(10);
        await client.ConnectAsync();

        var passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(passed).IsTrue();
    }

    [Test]
    public async Task ServerPortInUseTest()
    {
        using ManagedTcpListener server1 = new(ipEndPoint);
        _ = server1.StartListenAsync();

        await Assert.That(async () =>
        {
            using ManagedTcpListener server2 = new(ipEndPoint);
            _ = server2.StartListenAsync();
            await Task.CompletedTask;
        }).Throws<SocketException>();
    }

    [Test]
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
        client.DataReceived += async (object? sender, DataReceivedEventArgs e) =>
        {
            await Assert.That(e.Data).IsEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();

        var passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(passed).IsTrue();
        client.Dispose();

        tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

        client = new(ipEndPoint);
        client.DataReceived += async (object? sender, DataReceivedEventArgs e) =>
        {
            await Assert.That(e.Data).IsEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();

        passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(passed).IsTrue();
        client.Dispose();
    }

    [Test]
    public async Task ServerFailedTest()
    {
        using ManagedTcpListener server = new(ipEndPoint);
        var errorTcsS = new TaskCompletionSource<ErrorEventCode>(TaskCreationOptions.RunContinuationsAsynchronously);
        server.IncomingTcpClient += (object? sender, IncomingTcpClientEventArgs e) =>
        {
            e.TcpClient.Dispose();
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            errorTcsS.TrySetResult(e.ErrorCode);
        };
        var serverTask = server.StartListenAsync();

        using ManagedTcpClient client = new(ipEndPoint);
        var errorTcsC = new TaskCompletionSource<ErrorEventCode>(TaskCreationOptions.RunContinuationsAsynchronously);
        var dataReceived = false;
        client.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            dataReceived = true;
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            errorTcsC.TrySetResult(e.ErrorCode);
        };
        await client.ConnectAsync();

        await RetryHelper.WaitUntilWithTimeoutAsync(() => client.IsConnected, false);

        await Assert.That(async () =>
        {
            await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        }).Throws<InvalidOperationException>();

        var errorCode = await errorTcsC.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(errorCode).IsEqualTo(ErrorEventCode.ConnectionLost);
        await Assert.That(dataReceived).IsFalse();
    }

    [Test]
    public async Task NoServerTest()
    {
        using ManagedTcpClient client = new(ipEndPoint);

        await Assert.That(async () =>
        {
            await client.ConnectAsync(250);
        }).Throws<TimeoutException>();
    }

    [Test]
    public async Task StartAndStopSendAndReceiveData()
    {
        using ManagedTcpListener server = new(ipEndPoint);

        var serverTask = server.StartListenAsync();
        server.Stop();
        await Task.Delay(10);
        await Assert.That(serverTask.IsCompleted).IsTrue();

        server.IncomingTcpClient += async (object? sender, IncomingTcpClientEventArgs e) =>
        {
            await e.TcpClient.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        _ = server.StartListenAsync();
        await Assert.That(server.IsListening).IsTrue();

        using ManagedTcpClient client = new(ipEndPoint);
        client.DataReceived += async (object? sender, DataReceivedEventArgs e) =>
        {
            await Assert.That(e.Data).IsEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        client.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        await client.ConnectAsync();

        var passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(passed).IsTrue();
    }
}
