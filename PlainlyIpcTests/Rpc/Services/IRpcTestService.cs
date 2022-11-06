namespace PlainlyIpcTests.Rpc.Services;

public interface IRpcTestService
{
    public int Add(int a, int b);
    public void NoResultOp(int x);
    public Task<int> Sum(IEnumerable<int> values);
    public IEnumerable<int> Convert(params int[] values);
    public T Generic<T>(T value);
    public Task GetTask();
    public int ThrowError(string test);
}
