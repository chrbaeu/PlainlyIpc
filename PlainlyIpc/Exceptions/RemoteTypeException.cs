namespace PlainlyIpc.Exceptions;

public class RemoteTypeException : RemoteException
{
    public string Type { get; }
    public RemoteTypeException(string type, string message) : base(message) { Type = type; }

}
