using PlainlyIpc.NamedPipe;
using System.IO;

namespace PlainlyIpcTests.NamedPipe;

public class NamedPipeTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private readonly TaskCompletionSource<bool> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Fact]
    public async Task SendAndReceiveData()
    {
        using NamedPipeServer server = new(namedPipeName);
        server.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        var serverTask = Task.Run(() => server.StartAsync());

        using NamedPipeClient client = new(namedPipeName);
        await client.ConnectAsync();

        await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));

        passed.Should().BeTrue();
    }

    [Fact]
    public void NamedPipeInUseTest()
    {
        using NamedPipeServer server1 = new(namedPipeName);
        Assert.Throws<IOException>(() =>
        {
            using NamedPipeServer server2 = new(namedPipeName);
        });
    }

    [Fact]
    public async Task ConnectAndReconnectTest()
    {
        using NamedPipeServer server = new(namedPipeName);
        server.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            e.Data.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            if (e.ErrorCode != ErrorEventCode.ConnectionLost) { Assert.Fail(e.Message); }
        };
        var serverTask = Task.Run(() => server.StartAsync());

        NamedPipeClient client = new(namedPipeName);
        await client.ConnectAsync();
        client.Dispose();

        await Task.Delay(10);

        client = new(namedPipeName);
        await client.ConnectAsync();
        await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        client.Dispose();

        var passed = await tsc.Task.WaitAsync(new TimeSpan(0, 0, 5));

        passed.Should().BeTrue();
    }

    [Fact]
    public async Task ServerFailedTest()
    {
        NamedPipeServer server = new(namedPipeName);

        using NamedPipeClient client = new(namedPipeName);
        await client.ConnectAsync();

        server.Dispose();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        });
    }

    [Fact]
    public async Task NoServerTest()
    {
        using NamedPipeClient client = new(namedPipeName);

        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await client.ConnectAsync(250);
        });
    }

}
