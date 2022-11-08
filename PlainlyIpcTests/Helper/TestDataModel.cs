namespace PlainlyIpcTests.Helper;


[Serializable]
public record TestDataModel : ITestDataModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
