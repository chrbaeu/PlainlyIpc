namespace PlainlyIpcTests.Converter;

public class XmlConverterTests
{
#pragma warning disable CS0618 // Type or member is obsolete
    private readonly XmlObjectConverter converter = new();
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

    private static void ObjectSerializationDeserializationBaseTest<T>(IObjectConverter converter, T data) where T : class
    {
        byte[] serialized = converter.Serialize(data);
        T? deserialized = converter.Deserialize<T>(serialized);
        deserialized.Should().Be(data);
        deserialized = converter.Deserialize(serialized, typeof(T)) as T;
        deserialized.Should().Be(data);
    }

}
