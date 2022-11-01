namespace PlainlyIpc.EventArgs;

public sealed class ErrorOccurredEventArgs : System.EventArgs
{
    public int ErrorCode { get; }
    public string Message { get; }

    public Exception? Exception { get; }

    public ErrorOccurredEventArgs(int errorCode, string Message, Exception exception)
    {
        this.ErrorCode = errorCode;
        this.Message = Message;
        this.Exception = exception;
    }

}
