using System.Diagnostics;

namespace PlainlyIpcTests.Rpc;

internal class RpcTestService : IRpcTestService
{
    public int Add(int a, int b) => a + b;
    public void NoResultOp(int x) => Debug.WriteLine(x);
    public Task<int> Sum(IEnumerable<int> values) => Task.FromResult(values.Sum());
    public IEnumerable<int> Convert(params int[] values) => values;
    public T Generic<T>(T value) => value;
    public Task GetTask() => Task.CompletedTask;
    public int ThrowError(string test) => throw new ArgumentException("ERROR", nameof(test));
}
