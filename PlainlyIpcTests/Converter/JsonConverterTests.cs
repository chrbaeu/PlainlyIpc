namespace PlainlyIpcTests.Converter;

public class JsonConverterTests
{
    private readonly JsonObjectConverter converter = new();

    [Test]
    public async Task StringTest()
    {
        await ObjectSerializationDeserializationBaseTest(converter, TestData.Text);
    }

    [Test]
    public async Task ModelTest()
    {
        await ObjectSerializationDeserializationBaseTest(converter, TestData.Model);
    }

    [Test]
    public async Task InterfacesTest()
    {
        converter.AddInterfaceImplementation<ITestDataModel, TestDataModel>();

        byte[] serialized = converter.Serialize(TestData.TestDataModels);

        List<TestDataModel>? deserialized = converter.Deserialize<List<TestDataModel>>(serialized);
        await Assert.That(deserialized).IsEquivalentTo(TestData.TestDataModels);

        deserialized = converter.Deserialize(serialized, typeof(List<TestDataModel>)) as List<TestDataModel>;
        await Assert.That(deserialized).IsEquivalentTo(TestData.TestDataModels);
    }

    [Test]
    public async Task DictionaryTest()
    {
        byte[] serialized = converter.Serialize(TestData.Dict);

        Dictionary<string, long>? deserialized = converter.Deserialize<Dictionary<string, long>>(serialized);
        await Assert.That(deserialized).IsEquivalentTo(TestData.Dict);

        deserialized = converter.Deserialize(serialized, typeof(Dictionary<string, long>)) as Dictionary<string, long>;
        await Assert.That(deserialized).IsEquivalentTo(TestData.Dict);
    }

    private static async Task ObjectSerializationDeserializationBaseTest<T>(
        IObjectConverter converter,
        T data) where T : class
    {
        byte[] serialized = converter.Serialize(data);

        T? deserialized = converter.Deserialize<T>(serialized);
        await Assert.That(deserialized).IsEqualTo(data);

        deserialized = converter.Deserialize(serialized, typeof(T)) as T;
        await Assert.That(deserialized).IsEqualTo(data);
    }
}
