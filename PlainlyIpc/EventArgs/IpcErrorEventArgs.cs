using System;

namespace PlainlyIpc.EventArgs;

public class IpcErrorEventArgs : System.EventArgs
{
    public int ErrorCode { get; }
    public string Message { get; }

    public Exception? Exception { get; }

    public IpcErrorEventArgs(int errorCode, string Message, Exception exception)
    {
        this.ErrorCode = errorCode;
        this.Message = Message;
        this.Exception = exception;
    }

}
