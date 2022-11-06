namespace PlainlyIpc.Enums;

/// <summary>
/// Enum of the supported error event codes.
/// </summary>
public enum ErrorEventCode
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
    /// A event handlers has thrown an exception
    /// </summary>
    EventHandlerError = 1,

    /// <summary>
    /// An error occurred while processing data.
    /// </summary>
    DataProcessingError = 100,

    /// <summary>
    /// Unexpected error.
    /// </summary>
    UnexpectedError = 1000,
}
