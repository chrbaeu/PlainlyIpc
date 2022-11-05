namespace PlainlyIpcTests.Converter;

public class ConverterTests
{
    private readonly string testText = "Hello World";
    private readonly Dictionary<string, long> testDict = new() { { "A", 100 }, { "B", 200 } };
    private readonly TestDataModel testDataModel = new() { FirstName = "Max", LastName = "Mustermann" };

    [Fact]
    public void JsonObjectConverter_BaseTest()
    {
        JsonObjectConverter converter = new();
        ObjectSerializationDeserializationBaseTest(converter, testText);
        ObjectSerializationDeserializationBaseTest(converter, testDataModel);
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

    [Fact]
    public void BinaryObjectConverter_BaseTest()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        BinaryObjectConverter converter = new();
#pragma warning restore CS0618 // Type or member is obsolete
        ObjectSerializationDeserializationBaseTest(converter, testText);
        ObjectSerializationDeserializationBaseTest(converter, testDataModel);
    }

    [Fact]
    public void BinaryObjectConverter_DictionaryTest()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        BinaryObjectConverter converter = new();
#pragma warning restore CS0618 // Type or member is obsolete
        byte[] serialized = converter.Serialize(testDict);
        Dictionary<string, long>? deserialized = converter.Deserialize<Dictionary<string, long>>(serialized);
        deserialized.Should().BeEquivalentTo(testDict);
        deserialized = converter.Deserialize(serialized, typeof(Dictionary<string, long>)) as Dictionary<string, long>;
        deserialized.Should().BeEquivalentTo(testDict);
    }

    [Fact]
    public void XmlObjectConverter_BaseTest()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        XmlObjectConverter converter = new();
#pragma warning restore CS0618 // Type or member is obsolete
        ObjectSerializationDeserializationBaseTest(converter, testText);
        ObjectSerializationDeserializationBaseTest(converter, testDataModel);
    }

    private static void ObjectSerializationDeserializationBaseTest<T>(IObjectConverter converter, T data) where T : class
    {
        byte[] serialized = converter.Serialize(data);
        T? deserialized = converter.Deserialize<T>(serialized);
        deserialized.Should().Be(data);
        deserialized = converter.Deserialize(serialized, typeof(T)) as T;
        deserialized.Should().Be(data);
    }

}
