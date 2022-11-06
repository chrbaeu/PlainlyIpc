using PlainlyIpcTests.Rpc.Services;
using System.Net;

namespace PlainlyIpcTests.Rpc;

public class RpcTestServiceProxyTcpTest : IAsyncLifetime
{
    private readonly IPEndPoint ipEndPoint = ConnectionAddressFactory.GetIpEndPoint();
    private IIpcHandler server = null!;
    private IIpcHandler client = null!;
    private RpcTestServiceRemoteProxy proxy = null!;

    public async Task InitializeAsync()
    {
        IpcFactory ipcFactory = new();
        server = await ipcFactory.CreateTcpIpcServer(ipEndPoint);
        server.RegisterService<IRpcTestService>(new RpcTestService());
        client = await ipcFactory.CreateTcpIpcClient(ipEndPoint);
        proxy = new(client);
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
        var addResult = await proxy.AddAsync(4, 5);
        addResult.Should().Be(9);

        await proxy.NoResultOpAsync(4);

        var convertResult = await proxy.ConvertAsync(4, 5);
        convertResult.Should().NotBeNullOrEmpty();

#pragma warning disable CS0618 // Type or member is obsolete
        addResult = proxy.Add(4, 5);
        addResult.Should().Be(9);

        proxy.NoResultOp(4);

        convertResult = proxy.Convert(4, 5);
        convertResult.Should().NotBeNullOrEmpty();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public async Task GenericFunctionsTest()
    {
        var genericTextResult = await proxy.GenericAsync(TestData.Text);
        genericTextResult.Should().Be(TestData.Text);

        var genericDictResult = await proxy.GenericAsync(TestData.Dict);
        genericDictResult.Should().BeEquivalentTo(TestData.Dict);

        var genericModelResult = await proxy.GenericAsync(TestData.Model);
        genericModelResult.Should().Be(TestData.Model);

#pragma warning disable CS0618 // Type or member is obsolete
        genericTextResult = proxy.Generic(TestData.Text);
        genericTextResult.Should().Be(TestData.Text);

        genericDictResult = proxy.Generic(TestData.Dict);
        genericDictResult.Should().BeEquivalentTo(TestData.Dict);

        genericModelResult = proxy.Generic(TestData.Model);
        genericModelResult.Should().Be(TestData.Model);
#pragma warning restore CS0618 // Type or member is obsolete
    }


    [Fact]
    public async Task AsyncFunctionsTest()
    {
        var sumResult = await proxy.Sum(new int[] { 4, 5 });
        sumResult.Should().Be(9);

        await proxy.GetTask();
    }

    [Fact]
    public async Task ExceptionsTest()
    {
        await Assert.ThrowsAsync<RemoteException>(async () =>
        {
            var result = await proxy.ThrowErrorAsync("");
        });

        Assert.Throws<RemoteException>(() =>
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = proxy.ThrowError("");
#pragma warning restore CS0618 // Type or member is obsolete
        });
    }

}
