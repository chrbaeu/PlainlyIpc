namespace PlainlyIpcTests.Helper;

internal static class TestData
{
    public static string Text { get; } = "Hello World";
    public static Dictionary<string, long> Dict { get; } = new() { { "A", 100 }, { "B", 200 } };
    public static List<TestDataModel> TestDataModels { get; } = [
        new() { FirstName = "Max", LastName = "Mustermann" },
        new() { FirstName = "First", LastName = "Last" }
    ];
    public static TestDataModel Model { get; } = new() { FirstName = "Max", LastName = "Mustermann" };
}
