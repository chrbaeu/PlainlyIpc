using PlainlyIpc.NamedPipe;

namespace PlainlyIpcTests.NamedPipe;

public class NamedPipeTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private readonly TaskCompletionSource<bool> tsc = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Test]
    public async Task SendAndReceiveData()
    {
        using NamedPipeServer server = new(namedPipeName);
        server.DataReceived += async (object? sender, DataReceivedEventArgs e) =>
        {
            await Assert.That(e.Data).IsEquivalentTo(Encoding.UTF8.GetBytes(TestData.Text));
            tsc.SetResult(true);
        };
        var serverTask = Task.Run(() => server.StartAsync());

        using NamedPipeClient client = new(namedPipeName);
        await client.ConnectAsync();

        await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));

        var passed = await tsc.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(passed).IsTrue();
    }

    [Test]
    public async Task NamedPipeInUseTest()
    {
        using NamedPipeServer server1 = new(namedPipeName);

        await Assert.That(async () =>
        {
            using NamedPipeServer server2 = new(namedPipeName);
            await Task.CompletedTask;
        }).Throws<IOException>();
    }

    [Test]
    public async Task ConnectAndReconnectTest()
    {
        TaskCompletionSource<ErrorOccurredEventArgs> errorTsc = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<string> resultTsc = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using NamedPipeServer server = new(namedPipeName);
        server.DataReceived += (object? sender, DataReceivedEventArgs e) =>
        {
            resultTsc.SetResult(Encoding.UTF8.GetString(e.Data));
        };
        server.ErrorOccurred += (object? sender, ErrorOccurredEventArgs e) =>
        {
            errorTsc.TrySetResult(e);
        };
        var serverTask = Task.Run(() => server.StartAsync());

        NamedPipeClient client = new(namedPipeName);
        await client.ConnectAsync();
        await Task.Delay(100);
        client.Dispose();

        var erros = await errorTsc.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Assert.That(erros.ErrorCode).IsEqualTo(ErrorEventCode.ConnectionLost);

        client = new(namedPipeName);
        await client.ConnectAsync();
        await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        client.Dispose();

        var result = await resultTsc.Task.WaitAsync(TimeSpan.FromSeconds(10));

        await Assert.That(result).IsEqualTo(TestData.Text);
    }

    [Test]
    public async Task ServerFailedTest()
    {
        NamedPipeServer server = new(namedPipeName);

        using NamedPipeClient client = new(namedPipeName);
        await client.ConnectAsync();

        server.Dispose();

        await RetryHelper.WaitUntilWithTimeoutAsync(() => client.IsConnected, false);

        await Assert.That(async () =>
        {
            await client.SendAsync(Encoding.UTF8.GetBytes(TestData.Text));
        }).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task NoServerTest()
    {
        using NamedPipeClient client = new(namedPipeName);

        await Assert.That(async () =>
        {
            await client.ConnectAsync(250);
        }).Throws<TimeoutException>();
    }
}
