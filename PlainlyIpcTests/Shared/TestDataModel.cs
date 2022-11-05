namespace PlainlyIpcTests.Shared;


[Serializable]
public record TestDataModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
