namespace PlainlyIpc.Rpc;

internal class RemoteCall : RemoteMessage
{
    public RemoteCall(Guid uuid, Type interfaceType, string methodName, byte[][] parameters, Type[] genericArguments)
        : base(RemoteMsgType.RemoteCall, uuid)
    {
        InterfaceType = interfaceType;
        MethodName = methodName;
        Parameters = parameters;
        GenericArguments = genericArguments;
    }

    /// <summary>
    /// The interface used for the remote call.
    /// </summary>
    public Type InterfaceType { get; }

    /// <summary>
    /// The name of the method to invoke.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// The list of parameters to pass to the method.
    /// </summary>
    public byte[][] Parameters { get; }

    /// <summary>
    /// The types for the generic arguments.
    /// </summary>
    public Type[] GenericArguments { get; }

}
