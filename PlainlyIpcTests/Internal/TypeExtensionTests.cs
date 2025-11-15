namespace PlainlyIpcTests.Internal;

public class TypeExtensionsTests
{
    [Test]
    [Arguments(typeof(bool))]
    [Arguments(typeof(int))]
    [Arguments(typeof(string))]
    [Arguments(typeof(int[]))]
    [Arguments(typeof(int[][]))]
    [Arguments(typeof(int[][][]))]
    [Arguments(typeof((string, long)))]
    [Arguments(typeof(List<int>))]
    [Arguments(typeof(List<string>))]
    [Arguments(typeof(List<(string, long)>))]
    [Arguments(typeof(Dictionary<long, string>))]
    [Arguments(typeof(Dictionary<long, string[]>))]
    [Arguments(typeof(Dictionary<long, List<string>>))]
    [Arguments(typeof(List<List<string[][]>>))]
    public async Task TypeToStringAndBackTest(Type type)
    {
        var stringRepresentation = type.GetTypeString();
        var convertedType = PlainlyIpc.Internal.TypeExtensions.GetTypeFromTypeString(stringRepresentation);

        await Assert.That(convertedType).IsEqualTo(type);
    }
}
