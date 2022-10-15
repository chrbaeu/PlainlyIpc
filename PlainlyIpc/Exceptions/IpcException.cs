using System;

namespace PlainlyIpc.Exceptions;
public class IpcException : Exception
{
    public IpcException(string message) : base(message) { }
    public IpcException(string message, Exception innerException) : base(message, innerException) { }

}
