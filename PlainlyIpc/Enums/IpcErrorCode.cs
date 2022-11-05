namespace PlainlyIpc.Enums;

/// <summary>
/// Enum of the supported IPC error codes.
/// </summary>
public enum IpcErrorCode
{
    /// <summary>
    /// Connection is lost.
    /// </summary>
    ConnectionLost = -1,

    /// <summary>
    /// Not specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Unexpected error.
    /// </summary>
    UnexpectedError = 1,
}
