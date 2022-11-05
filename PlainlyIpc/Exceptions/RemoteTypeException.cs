using System.Diagnostics.CodeAnalysis;

namespace PlainlyIpc.Exceptions;

/// <summary>
/// Exception class for RPC type errors.
/// </summary>
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Exception is only for type related problems and requires a type name.")]
public sealed class RemoteTypeException : RemoteException
{
    /// <summary>
    /// The type that causes the exception.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Creates a new RPC exception with a type name and message.
    /// </summary>
    /// <param name="typeName">The name of type that causes the exception.</param>
    /// <param name="message">The error message.</param>
    public RemoteTypeException(string typeName, string message) : base(message) { TypeName = typeName; }

}
