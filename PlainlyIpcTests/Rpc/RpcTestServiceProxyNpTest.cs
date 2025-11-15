using PlainlyIpcTests.Rpc.Services;

namespace PlainlyIpcTests.Rpc;

public class RpcTestServiceProxyNpTest
{
    private readonly string namedPipeName = ConnectionAddressFactory.GetNamedPipeName();
    private IIpcHandler server = null!;
    private IIpcHandler client = null!;
    private MyRpcTestServiceRemoteProxy proxy = null!;

    [Before(Test)]
    public async Task InitializeAsync()
    {
        JsonObjectConverter converter = new();
        converter.AddInterfaceImplementation<ITestDataModel, TestDataModel>();
        IpcFactory ipcFactory = new(converter);
        server = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
        server.RegisterService<IRpcTestService>(new RpcTestService());
        client = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);
        proxy = new(client);
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
        var addResult = await proxy.AddAsync(4, 5);
        await Assert.That(addResult).IsEqualTo(9);

        await proxy.NoResultOpAsync(4);

        var convertResult = await proxy.ConvertAsync(4, 5);
        await Assert.That(convertResult).IsNotEmpty();

#pragma warning disable CS0618
        addResult = proxy.Add(4, 5);
        await Assert.That(addResult).IsEqualTo(9);

        proxy.NoResultOp(4);

        convertResult = proxy.Convert(4, 5);
        await Assert.That(convertResult).IsNotEmpty();
#pragma warning restore CS0618
    }

    [Test]
    public async Task GenericFunctionsTest()
    {
        var genericTextResult = await proxy.GenericAsync(TestData.Text);
        await Assert.That(genericTextResult).IsEqualTo(TestData.Text);

        var genericDictResult = await proxy.GenericAsync(TestData.Dict);
        await Assert.That(genericDictResult).IsEquivalentTo(TestData.Dict);

        var genericModelResult = await proxy.GenericAsync(TestData.Model);
        await Assert.That(genericModelResult).IsEqualTo(TestData.Model);

#pragma warning disable CS0618
        genericTextResult = proxy.Generic(TestData.Text);
        await Assert.That(genericTextResult).IsEqualTo(TestData.Text);

        genericDictResult = proxy.Generic(TestData.Dict);
        await Assert.That(genericDictResult).IsEquivalentTo(TestData.Dict);

        genericModelResult = proxy.Generic(TestData.Model);
        await Assert.That(genericModelResult).IsEqualTo(TestData.Model);
#pragma warning restore CS0618
    }

    [Test]
    public async Task AsyncFunctionsTest()
    {
        var sumResult = await proxy.Sum([4, 5]);
        await Assert.That(sumResult).IsEqualTo(9);

        await proxy.GetTask();
    }

    [Test]
    public async Task ExceptionsTest()
    {
        await Assert.That(async () =>
        {
            _ = await proxy.ThrowErrorAsync("");
        }).Throws<RemoteException>();

        await Assert.That(() =>
        {
#pragma warning disable CS0618
            _ = proxy.ThrowError("");
#pragma warning restore CS0618
        }).Throws<RemoteException>();
    }

    [Test]
    public async Task InterfaceTest()
    {
        var result = await proxy.Roundtrip(TestData.Model);
        await Assert.That(result).IsEqualTo(TestData.Model);
    }
}
