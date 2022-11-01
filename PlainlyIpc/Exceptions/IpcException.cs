﻿namespace PlainlyIpc.Exceptions;
public sealed class IpcException : Exception
{
    public IpcException(string message) : base(message) { }
    public IpcException(string message, Exception innerException) : base(message, innerException) { }

}