namespace PlainlyIpc.EventArgs;

/// <summary>
/// Event class for occurring errors.
/// </summary>
public sealed class ErrorOccurredEventArgs : System.EventArgs
{
    /// <summary>
    /// The error code.
    /// </summary>
    public ErrorEventCode ErrorCode { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The exception that causes the error (optional).
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Creates a new event for an occurring error.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The exception that causes the error (optional).</param>
    public ErrorOccurredEventArgs(ErrorEventCode errorCode, string message, Exception? exception)
    {
        ErrorCode = errorCode;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    /// A readable formatted error message string for the error event.
    /// </summary>
    /// <returns></returns>
    public string FormattedErrorMessage => $"{ErrorCode}: {Message} {Exception?.Message}";

}
