using System.IO;
using System.Linq.Expressions;

namespace PlainlyIpc.Rpc;

internal static class RemoteMessageHelper
{
    /// <summary>
    /// Converts a call expression to a remote call.
    /// </summary>
    /// <param name="interfaceType">The interfaces that defines the called method.</param>
    /// <param name="expression">The expression for the call.</param>
    /// <param name="objectConverter">The converter used to serialize the data.</param>
    /// <returns>The remote call.</returns>
    public static RemoteCall FromCall(Type interfaceType, Expression expression, IObjectConverter objectConverter)
    {
        if (expression is not LambdaExpression lambdaExp)
        {
            throw new ArgumentException("Only supports lambda expressions, e.g.: x => x.GetData(a, b)");
        }
        if (lambdaExp.Body is not MethodCallExpression methodCallExp)
        {
            throw new ArgumentException("Only supports calling methods, e.g.: x => x.GetData(a, b)");
        }
        string methodName = methodCallExp.Method.Name;
        object?[] argumentList = methodCallExp.Arguments.Select(argumentExpression => Expression.Lambda(argumentExpression).Compile().DynamicInvoke()).ToArray();
        byte[][] parameters = argumentList.Select(x => objectConverter.Serialize(x)).ToArray();
        Type[] genericArguments = methodCallExp.Method.GetGenericArguments();
        return new RemoteCall(Guid.NewGuid(), interfaceType, methodName, parameters, genericArguments);
    }

    /// <summary>
    /// Creates a remote message from a byte array.
    /// </summary>
    /// <param name="data">The byte array to parse.</param>
    /// <returns>The remote message.</returns>
    public static RemoteMessage FromBytes(byte[] data)
    {
        using MemoryStream memoryStream = new(data);
        var len = memoryStream.ReadLong();
        if (len != data.Length) { throw new ArgumentException("Invalid data!", nameof(data)); }
        var msgType = (RemoteMsgType)memoryStream.ReadInt();
        var uuid = new Guid(memoryStream.ReadArray());
        switch (msgType)
        {
            case RemoteMsgType.RemoteCall:
                var interfaceTypeName = memoryStream.ReadUtf8String();
                var interfaceType = TypeExtensions.GetTypeFromTypeString(interfaceTypeName);
                if (interfaceType is null) { throw new RemoteTypeException(interfaceTypeName, $"Type '{interfaceTypeName}' not found."); }
                var methodName = memoryStream.ReadUtf8String();
                var parameters = memoryStream.ReadArrayArray();
                var genericArguments = new Type[memoryStream.ReadInt()];
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    var typeName = memoryStream.ReadUtf8String();
                    var type = TypeExtensions.GetTypeFromTypeString(typeName);
                    if (type is null) { throw new RemoteTypeException(interfaceTypeName, $"Type '{typeName}' not found."); }
                    genericArguments[i] = type;
                }
                return new RemoteCall(uuid, interfaceType, methodName, parameters, genericArguments);
            case RemoteMsgType.RemoteResult:
                return new RemoteResult(uuid, memoryStream.ReadArray());
            case RemoteMsgType.RemoteError:
                return new RemoteError(uuid, memoryStream.ReadUtf8String());
            default:
                throw new NotSupportedException($"MsgType {msgType} is not supported!");
        }
    }

    /// <summary>
    /// Extension method to convert messages to a byte array.
    /// </summary>
    /// <returns>The byte array representation of the message.</returns>
    public static byte[] AsBytes(this RemoteMessage remoteMessage)
    {
        using MemoryStream memoryStream = new();
        memoryStream.WriteLong(0);
        memoryStream.WriteInt((int)remoteMessage.MsgType);
        memoryStream.WriteArray(remoteMessage.Uuid.ToByteArray());
        switch (remoteMessage)
        {
            case RemoteCall remoteCall:
                memoryStream.WriteUtf8String(remoteCall.InterfaceType.GetTypeString());
                memoryStream.WriteUtf8String(remoteCall.MethodName);
                memoryStream.WriteArrayArray(remoteCall.Parameters);
                memoryStream.WriteInt(remoteCall.GenericArguments.Length);
                foreach (Type genericArgument in remoteCall.GenericArguments)
                {
                    memoryStream.WriteUtf8String(genericArgument.GetTypeString());
                }
                break;
            case RemoteResult remoteResult:
                memoryStream.WriteArray(remoteResult.ResultData);
                break;
            case RemoteError remoteError:
                memoryStream.WriteUtf8String(remoteError.ErrorMessage);
                break;
            default:
                throw new NotSupportedException($"MsgType {remoteMessage.MsgType} is not supported!");
        }
        memoryStream.Position = 0;
        memoryStream.WriteLong(memoryStream.Length);
        byte[] messageBytes = memoryStream.ToArray();
        return messageBytes;
    }

}
