namespace PlainlyIpc.Enums;

public enum IpcMessageType : byte
{
    RawData,
    StringData,
    ObjectData,
    RemoteMessage,
}
