namespace PlainlyIpc.Exceptions;

/// <summary>
/// Exception class for RPC errors.
/// </summary>
public class RemoteException : Exception
{
    /// <summary>
    /// Creates a new RPC exception.
    /// </summary>
    public RemoteException() : base() { }

    /// <summary>
    /// Creates a new RPC exception with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RemoteException(string message) : base(message) { }

    /// <summary>
    /// Creates a new RPC exception with message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RemoteException(string message, Exception innerException) : base(message, innerException) { }

}
