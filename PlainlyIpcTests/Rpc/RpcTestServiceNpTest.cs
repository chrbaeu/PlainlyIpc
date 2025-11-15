using PlainlyIpcTests.Rpc.Services;

namespace PlainlyIpcTests.Rpc;

public class RpcTestServiceNpTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private IIpcHandler server = null!;
    private IIpcHandler client = null!;

    [Before(Test)]
    public async Task InitializeAsync()
    {
        JsonObjectConverter converter = new();
        converter.AddInterfaceImplementation<ITestDataModel, TestDataModel>();
        IpcFactory ipcFactory = new(converter);
        server = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        server.RegisterService<IRpcTestService>(new RpcTestService());
        client = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);
    }

    [After(Test)]
    public Task DisposeAsync()
    {
        client?.Dispose();
        server?.Dispose();
        return Task.CompletedTask;
    }

    [Test]
    public async Task BasicFunctionsTest()
    {
        var addResult = await client.ExecuteRemote<IRpcTestService, int>(x => x.Add(4, 5));
        await Assert.That(addResult).IsEqualTo(9);

        await client.ExecuteRemote<IRpcTestService>(x => x.NoResultOp(4));

        var convertResult = await client.ExecuteRemote<IRpcTestService, IEnumerable<int>>(x => x.Convert(4, 5));
        await Assert.That(convertResult).IsNotEmpty();
    }

    [Test]
    public async Task GenericFunctionsTest()
    {
        var genericTextResult = await client.ExecuteRemote<IRpcTestService, string>(x => x.Generic(TestData.Text));
        await Assert.That(genericTextResult).IsEqualTo(TestData.Text);

        var genericDictResult = await client.ExecuteRemote<IRpcTestService, Dictionary<string, long>>(x => x.Generic(TestData.Dict));
        await Assert.That(genericDictResult).IsEquivalentTo(TestData.Dict);

        var genericModelResult = await client.ExecuteRemote<IRpcTestService, TestDataModel>(x => x.Generic(TestData.Model));
        await Assert.That(genericModelResult).IsEqualTo(TestData.Model);

        var guid = Guid.NewGuid();
        var genericGuidResult = await client.ExecuteRemote<IRpcTestService, Guid>(x => x.Generic(guid));
        await Assert.That(genericGuidResult).IsEqualTo(guid);
    }

    [Test]
    public async Task AsyncFunctionsTest()
    {
        int[] values = [4, 5];
        var sumResult = await client.ExecuteRemote<IRpcTestService, int>(x => x.Sum(values));
        await Assert.That(sumResult).IsEqualTo(9);

        await client.ExecuteRemote<IRpcTestService>(x => x.GetTask());
    }

    [Test]
    public async Task ExceptionsTest()
    {
        await Assert.That(async () =>
        {
            _ = await client.ExecuteRemote<IRpcTestService, int>(x => x.ThrowError(""));
        }).Throws<RemoteException>();
    }

    [Test]
    public async Task InterfaceTest()
    {
        var result = await client.ExecuteRemote<IRpcTestService, ITestDataModel>(x => x.Roundtrip(TestData.Model));
        await Assert.That(result).IsEqualTo(TestData.Model);
    }
}
