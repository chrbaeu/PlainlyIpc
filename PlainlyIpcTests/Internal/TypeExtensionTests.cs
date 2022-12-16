namespace PlainlyIpcTests.Internal;

public class TypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(bool))]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(int[][]))]
    [InlineData(typeof(int[][][]))]
    [InlineData(typeof((string, long)))]
    [InlineData(typeof(List<int>))]
    [InlineData(typeof(List<string>))]
    [InlineData(typeof(List<(string, long)>))]
    [InlineData(typeof(Dictionary<long, string>))]
    [InlineData(typeof(Dictionary<long, string[]>))]
    [InlineData(typeof(Dictionary<long, List<string>>))]
    [InlineData(typeof(List<List<string[][]>>))]
    public void TypeToStringAndBackTest(Type type)
    {
        var stringRepresentation = type.GetTypeString();
        var convertedType = PlainlyIpc.Internal.TypeExtensions.GetTypeFromTypeString(stringRepresentation);
        convertedType.Should().Be(type);
    }

}
