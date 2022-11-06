using PlainlyIpcTests.Rpc.Services;

namespace PlainlyIpcTests.Rpc;

public class RpcTestServiceNpTest : IAsyncLifetime
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private IIpcHandler server = null!;
    private IIpcHandler client = null!;

    public async Task InitializeAsync()
    {
        IpcFactory ipcFactory = new();
        server = await ipcFactory.CreateNampedPipeIpcServer(namedPipeName);
        server.RegisterService<IRpcTestService>(new RpcTestService());
        client = await ipcFactory.CreateNampedPipeIpcClient(namedPipeName);
    }

    public Task DisposeAsync()
    {
        client?.Dispose();
        server?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task BasicFunctionsTest()
    {
        var addResult = await client.ExecuteRemote<IRpcTestService, int>(x => x.Add(4, 5));
        addResult.Should().Be(9);

        await client.ExecuteRemote<IRpcTestService>(x => x.NoResultOp(4));

        var convertResult = await client.ExecuteRemote<IRpcTestService, IEnumerable<int>>(x => x.Convert(4, 5));
        convertResult.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenericFunctionsTest()
    {
        var genericTextResult = await client.ExecuteRemote<IRpcTestService, string>(x => x.Generic(TestData.Text));
        genericTextResult.Should().Be(TestData.Text);

        var genericDictResult = await client.ExecuteRemote<IRpcTestService, Dictionary<string, long>>(x => x.Generic(TestData.Dict));
        genericDictResult.Should().BeEquivalentTo(TestData.Dict);

        var genericModelResult = await client.ExecuteRemote<IRpcTestService, TestDataModel>(x => x.Generic(TestData.Model));
        genericModelResult.Should().Be(TestData.Model);
    }


    [Fact]
    public async Task AsyncFunctionsTest()
    {
        var sumResult = await client.ExecuteRemote<IRpcTestService, int>(x => x.Sum(new int[] { 4, 5 }));
        sumResult.Should().Be(9);

        await client.ExecuteRemote<IRpcTestService>(x => x.GetTask());
    }

    [Fact]
    public async Task ExceptionsTest()
    {
        await Assert.ThrowsAsync<RemoteException>(async () =>
        {
            var result = await client.ExecuteRemote<IRpcTestService, int>(x => x.ThrowError(""));
        });
    }

}
