namespace PlainlyIpc.Enums;

/// <summary>
/// Enum of the supported IPC message types.
/// </summary>
public enum IpcMessageType : byte
{
    RawData,
    StringData,
    ObjectData,
    RemoteMessage,
}
