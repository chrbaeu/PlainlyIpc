﻿namespace PlainlyIpcTests.Rpc.Services;

[PlainlyIpc.SourceGenerator.GenerateProxy]
public interface IRpcTestService
{
    public int Add(int a, int b);
    public void NoResultOp(int x);
    public Task<int> Sum(IEnumerable<int> values);
    public IEnumerable<int> Convert(params int[] values);
    public T Generic<T>(T value);
    public Task GetTask();
    public int ThrowError(string test);
    public Task<ITestDataModel> Roundtrip(ITestDataModel dataModel);
}
