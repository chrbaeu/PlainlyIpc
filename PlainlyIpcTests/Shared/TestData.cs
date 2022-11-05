namespace PlainlyIpcTests.Shared;

internal class TestData
{
    public static string Text { get; } = "Hello World";
    public static Dictionary<string, long> Dict { get; } = new() { { "A", 100 }, { "B", 200 } };
    public static TestDataModel Model { get; } = new() { FirstName = "Max", LastName = "Mustermann" };
}
