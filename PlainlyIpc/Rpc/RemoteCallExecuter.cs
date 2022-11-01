using System.Reflection;

namespace PlainlyIpc.Rpc;

internal static class RemoteCallExecuter
{
    public static async Task<RemoteMessage> Execute(RemoteCall remoteCall, object instance, IObjectConverter objectConverter)
    {
        if (!remoteCall.InterfaceType.IsAssignableFrom(instance.GetType()))
        {
            return new RemoteError(remoteCall.Uuid, $"Instance is no valid implementation of the interface '{remoteCall.InterfaceType.FullName}'.");
        }
        MethodInfo? method = instance.GetType().GetRuntimeMethods().FirstOrDefault(x => x.Name.Split('.').Last() == remoteCall.MethodName);
        if (method == null)
        {
            return new RemoteError(remoteCall.Uuid, $"Method '{remoteCall.MethodName}' not found in interface '{instance.GetType().FullName}'.");
        }
        ParameterInfo[] paramInfoList = method.GetParameters();
        if (paramInfoList.Length != remoteCall.Parameters.Length)
        {
            return new RemoteError(remoteCall.Uuid, $"Parameter count mismatch for method '{remoteCall.MethodName}'.");
        }
        Type[] genericArguments = method.GetGenericArguments();
        if (genericArguments.Length != remoteCall.GenericArguments.Length)
        {
            return new RemoteError(remoteCall.Uuid, $"Generic argument count mismatch for method '{remoteCall.MethodName}'.");
        }
        if (paramInfoList.Any(info => info.IsOut || info.ParameterType.IsByRef))
        {
            return new RemoteError(remoteCall.Uuid, $"Method '{remoteCall.MethodName}' is not supported, because ref and out parameters are not supported.");
        }
        object?[] parameters = new object[paramInfoList.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            Type destType = paramInfoList[i].ParameterType;
            if (destType.IsGenericParameter)
            {
                destType = remoteCall.GenericArguments[destType.GenericParameterPosition];
            }
            byte[] parameterBytes = remoteCall.Parameters[i];
            parameters[i] = objectConverter.Deserialize(parameterBytes, destType);
        }
        try
        {
            if (method.IsGenericMethod)
            {
                method = method.MakeGenericMethod(remoteCall.GenericArguments);
            }
            object? resultOrTask = method.Invoke(instance, parameters);
            object? result = null;
#if NET6_0_OR_GREATER
            if (resultOrTask is ValueTask valueTask)
            {
                resultOrTask = valueTask.AsTask();
            }
#endif
            if (resultOrTask is Task task)
            {
                await task.ConfigureAwait(false);
                if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultProperty = resultOrTask.GetType().GetProperty("Result");
                    result = resultProperty?.GetValue(resultOrTask);
                }
            }
            else if (resultOrTask is IAsyncResult)
            {
                throw new InvalidOperationException("Return values of type IAsyncResult are not supported.");
            }
            else
            {
                result = resultOrTask;
            }
            return new RemoteResult(remoteCall.Uuid, objectConverter.Serialize(result));
        }
        catch (Exception exception)
        {
            return new RemoteError(remoteCall.Uuid, exception.ToString());
        }
    }

}
