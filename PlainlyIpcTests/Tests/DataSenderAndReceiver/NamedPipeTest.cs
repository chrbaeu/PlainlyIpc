using PlainlyIpc.EventArgs;
using PlainlyIpc.NamedPipe;
using System.Text;

namespace PlainlyIpcTests.Tests.NamedPipe;

public class NamedPipeTest
{
    private readonly string testText = "Hello World";

    [Fact]
    public async Task SendAndReciveData()
    {
        TaskCompletionSource<bool> tsc = new();

        using NamedPipeServer server = new("PlainlyIpcTests_SendAndReciveData");
        server.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(testText));
            tsc.SetResult(true);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

        using NamedPipeClient client = new("PlainlyIpcTests_SendAndReciveData");
        await client.ConnectAsync();

        await client.SendAsync(Encoding.UTF8.GetBytes(testText));

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));

        passed.Should().BeTrue();
    }

    [Fact]
    public void NamedPipeInUseTest()
    {
        using NamedPipeServer server1 = new("PlainlyIpcTests_NamedPipeInUseTest");
        Assert.Throws<IOException>(() =>
        {
            using NamedPipeServer server2 = new("PlainlyIpcTests_NamedPipeInUseTest");
        });
    }

    [Fact]
    public async Task ConnectAndReconnectTest()
    {
        TaskCompletionSource<bool> tsc = new();

        using NamedPipeServer server = new("PlainlyIpcTests_ConnectAndReconnectTest");
        server.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(testText));
            tsc.SetResult(true);
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            Assert.Fail(e.Message);
        };
        var serverTask = Task.Run(() => server.StartListenAync());

        NamedPipeClient client = new("PlainlyIpcTests_ConnectAndReconnectTest");
        await client.ConnectAsync();
        client.Dispose();

        client = new("PlainlyIpcTests_ConnectAndReconnectTest");
        await client.ConnectAsync();
        await client.SendAsync(Encoding.UTF8.GetBytes(testText));
        client.Dispose();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 1));

        passed.Should().BeTrue();
    }

    [Fact]
    public async Task ServerFailedTest()
    {
        NamedPipeServer server = new("PlainlyIpcTests_ServerFailedTest");

        using NamedPipeClient client = new("PlainlyIpcTests_ServerFailedTest");
        await client.ConnectAsync();

        server.Dispose();

        await Assert.ThrowsAsync<IOException>(async () =>
        {
            await client.SendAsync(Encoding.UTF8.GetBytes(testText));
        });
    }

    [Fact]
    public async Task NoServerTest()
    {
        using NamedPipeClient client = new("PlainlyIpcTests_NoServerTest");

        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await client.ConnectAsync(250);
        });
    }

}
