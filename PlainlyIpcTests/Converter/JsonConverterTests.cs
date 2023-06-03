namespace PlainlyIpcTests.Converter;

public class JsonConverterTests
{
    private readonly JsonObjectConverter converter = new();

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
    public void InterfacesTest()
    {
        converter.AddInterfaceImplementation<ITestDataModel, TestDataModel>();
        byte[] serialized = converter.Serialize(TestData.TestDataModels);
        List<TestDataModel>? deserialized = converter.Deserialize<List<TestDataModel>>(serialized);
        deserialized.Should().BeEquivalentTo(TestData.TestDataModels);
        deserialized = converter.Deserialize(serialized, typeof(List<TestDataModel>)) as List<TestDataModel>;
        deserialized.Should().BeEquivalentTo(TestData.TestDataModels);
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
