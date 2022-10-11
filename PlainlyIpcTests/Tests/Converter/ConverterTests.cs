using PlainlyIpc.Converter;

namespace PlainlyIpcTests.Tests.Converter;

public class ConverterTests
{
    private readonly string testText = "Hello World";
    private readonly Dictionary<string, long> testDict = new()
    {
        { "A", 100 },
        { "B", 200 },
    };

    public ConverterTests()
    {
    }

    [Fact]
    public void BinaryObjectConverter_StringTest()
    {
        BinaryObjectConverter converter = new();
        byte[] serialized = converter.Serialize(testText);
        string? deserialized = converter.Deserialize<string>(serialized);
        deserialized.Should().Be(testText);
        deserialized = converter.Deserialize(serialized, typeof(string)) as string;
        deserialized.Should().Be(testText);
    }

    [Fact]
    public void BinaryObjectConverter_DictionaryTest()
    {
        BinaryObjectConverter converter = new();
        byte[] serialized = converter.Serialize(testDict);
        Dictionary<string, long>? deserialized = converter.Deserialize<Dictionary<string, long>>(serialized);
        deserialized.Should().BeEquivalentTo(testDict);
        deserialized = converter.Deserialize(serialized, typeof(Dictionary<string, long>)) as Dictionary<string, long>;
        deserialized.Should().BeEquivalentTo(testDict);
    }

    [Fact]
    public void JsonObjectConverter_StringTest()
    {
        JsonObjectConverter converter = new();
        byte[] serialized = converter.Serialize(testText);
        string? deserialized = converter.Deserialize<string>(serialized);
        deserialized.Should().Be(testText);
        deserialized = converter.Deserialize(serialized, typeof(string)) as string;
        deserialized.Should().Be(testText);
    }

    [Fact]
    public void JsonObjectConverter_DictionaryTest()
    {
        JsonObjectConverter converter = new();
        byte[] serialized = converter.Serialize(testDict);
        Dictionary<string, long>? deserialized = converter.Deserialize<Dictionary<string, long>>(serialized);
        deserialized.Should().BeEquivalentTo(testDict);
        deserialized = converter.Deserialize(serialized, typeof(Dictionary<string, long>)) as Dictionary<string, long>;
        deserialized.Should().BeEquivalentTo(testDict);
    }

}
