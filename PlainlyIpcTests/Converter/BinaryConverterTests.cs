namespace PlainlyIpcTests.Converter;

public class BinaryConverterTests
{
#pragma warning disable CS0618 // Type or member is obsolete
    private readonly BinaryObjectConverter converter = new();
#pragma warning restore CS0618 // Type or member is obsolete

    [Fact]
    public void StringTest()
    {
        ObjectSerializationDeserializationBaseTest(converter, TestData.Text);
    }

    [Fact]
    public void ModelTest()
    {
        ObjectSerializationDeserializationBaseTest(converter, TestData.Model);
    }

    [Fact]
    public void DictionaryTest()
    {
        byte[] serialized = converter.Serialize(TestData.Dict);
        Dictionary<string, long>? deserialized = converter.Deserialize<Dictionary<string, long>>(serialized);
        deserialized.Should().BeEquivalentTo(TestData.Dict);
        deserialized = converter.Deserialize(serialized, typeof(Dictionary<string, long>)) as Dictionary<string, long>;
        deserialized.Should().BeEquivalentTo(TestData.Dict);
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
