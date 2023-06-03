using System.Diagnostics.CodeAnalysis;

namespace PlainlyIpc.Enums;

/// <summary>
/// Enum of the supported IPC message types.
/// </summary>
[SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Byte based so that only one byte is needed for the transmission.")]
public enum IpcMessageType : byte
{
    /// <summary>
    /// A raw byte arrays
    /// </summary>
    RawData,

    /// <summary>
    /// A uft-8 encoded string
    /// </summary>
    StringData,

    /// <summary>
    /// A serialized object
    /// </summary>
    ObjectData,

    /// <summary>
    /// A RPC message
    /// </summary>
    RemoteMessage,
}
