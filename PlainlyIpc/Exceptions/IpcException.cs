namespace PlainlyIpc.Exceptions;

/// <summary>
/// Exception class for IPC errors.
/// </summary>
public sealed class IpcException : Exception
{
    /// <summary>
    /// Creates a new IPC exception.
    /// </summary>
    public IpcException() : base() { }

    /// <summary>
    /// Creates a new IPC exception with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public IpcException(string message) : base(message) { }

    /// <summary>
    /// Creates a new IPC exception with message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public IpcException(string message, Exception innerException) : base(message, innerException) { }

}
