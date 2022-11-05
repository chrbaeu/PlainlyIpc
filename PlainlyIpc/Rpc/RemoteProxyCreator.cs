using System.IO;

namespace PlainlyIpc.Rpc;

/// <summary>
/// Class for creating proxy classes for RPC interfaces.  
/// </summary>
public static class RemoteProxyCreator
{
    /// <summary>
    /// Gets the class name for the RPC proxy.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <returns></returns>
    public static string GetProxyClassName<TInterface>()
    {
        var type = typeof(TInterface);
        var className = $"{type.Name}RemoteProxy";
        if (className.StartsWith("I", StringComparison.Ordinal)) { className = className.Substring(1); }
        return className;
    }

    /// <summary>
    /// Saves the source code file for a proxy classes for the RPC interfaces to the given folder. 
    /// </summary>
    /// <typeparam name="TInterface">The RPC interface.</typeparam>
    /// <param name="outputFolderPath">The output folder Path.</param>
    /// <param name="baseNamespace">"The base namespace."</param>
    public static void CreateProxyClass<TInterface>(string outputFolderPath, string baseNamespace)
    {
        var src = CreateProxyClass<TInterface>(baseNamespace);
        File.WriteAllText(Path.Combine(Path.GetFullPath(outputFolderPath), GetProxyClassName<TInterface>() + ".cs"), src);
    }

    /// <summary>
    /// Creates the source code for a proxy classes for the RPC interfaces. 
    /// </summary>
    /// <typeparam name="TInterface">The RPC interface.</typeparam>
    /// <param name="baseNamespace">"The base namespace."</param>
    /// <returns>The source code for the proxy class.</returns>
    public static string CreateProxyClass<TInterface>(string baseNamespace)
    {
        var type = typeof(TInterface);
        var className = GetProxyClassName<TInterface>();
        StringBuilder sb = new();
        sb.AppendLine($$"""
            using System;
            using System.Threading.Tasks;

            namespace {{baseNamespace}}.PlainlyIpcProxies;

            """);
        sb.AppendLine($$"""
            public class {{className}} : {{type.FullName}}
            {
                private readonly IIpcHandler ipcHandler;

                public {{className}}(IIpcHandler ipcHandler)
                {
                    this.ipcHandler = ipcHandler;
                }

            """);
        foreach (var method in type.GetMethods())
        {
            string methodName = method.Name;
            string asyncMethodName = methodName;
            string genericArgumentsStatement = "";
            Type[] genericArguments = method.GetGenericArguments();
            if (genericArguments.Any())
            {
                genericArgumentsStatement = $"<{string.Join(", ", genericArguments.Select(x => TrimNamespace(GetTypeDefinition(x))))}>";
            }
            var parameterInfos = method.GetParameters();
            string paramDefs = string.Join(", ", parameterInfos.Select(x =>
                $"{(x.IsDefined(typeof(ParamArrayAttribute), false) ? "params " : "")}{TrimNamespace(GetTypeDefinition(x.ParameterType))} {x.Name}"));
            string paramVals = string.Join(", ", parameterInfos.Select(x => x.Name));
            var returnType = GetTypeDefinition(method.ReturnType);
            var remoteReturnType = returnType.StartsWith("System.Threading.Tasks.Task<", StringComparison.OrdinalIgnoreCase)
                ? TrimNamespace(returnType.Substring(28, returnType.Length - 29))
                : TrimNamespace(returnType);
            var remoteGenerics = $"{TrimNamespace(GetTypeDefinition(type))}, {remoteReturnType}";
            string returnStatement = "return ";
#if NETSTANDARD2_0
            returnType = returnType.Replace("System.Threading.Tasks.ValueTask", "System.Threading.Tasks.Task");
#else
            returnType = returnType.Replace("System.Threading.Tasks.ValueTask", "System.Threading.Tasks.Task", StringComparison.Ordinal);
#endif
            if (returnType == "System.Void")
            {
                returnStatement = "";
                returnType = "Task";
                remoteGenerics = $"{TrimNamespace(GetTypeDefinition(type))}";
                asyncMethodName += "Async";
                sb.AppendLine($$"""
                [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
                public void {{methodName}}{{genericArgumentsStatement}}({{paramDefs}})
                {
                    Task.Run(() => {{asyncMethodName}}({{paramVals}})).GetAwaiter().GetResult();
                }

            """);
            }
            else if (returnType != "System.Threading.Tasks.Task"
                && !returnType.StartsWith("System.Threading.Tasks.Task<", StringComparison.Ordinal))
            {
                returnType = $"Task<{TrimNamespace(returnType)}>";
                asyncMethodName += "Async";
                sb.AppendLine($$"""
                [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
                public {{remoteReturnType}} {{methodName}}{{genericArgumentsStatement}}({{paramDefs}})
                {
                    return Task.Run(() => {{asyncMethodName}}({{paramVals}})).GetAwaiter().GetResult();
                }

            """);
            }
            else if (returnType == "System.Threading.Tasks.Task")
            {
                returnStatement = "";
            }
            sb.AppendLine($$"""
                public async {{TrimNamespace(returnType)}} {{asyncMethodName}}{{genericArgumentsStatement}}({{paramDefs}})
                {
                    {{returnStatement}}await ipcHandler.ExecuteRemote<{{remoteGenerics}}>(plainlyRpcService
                        => plainlyRpcService.{{methodName}}({{paramVals}}));
                }

            """);
        }
        sb.AppendLine($$"""
            }

            """);
        return sb.ToString();
    }


    private static string TrimNamespace(string typeDefinition)
    {
        if (typeDefinition.StartsWith("System.Threading.Tasks.", StringComparison.Ordinal))
        {
            return typeDefinition.Substring("System.Threading.Tasks.".Length);
        }
        if (typeDefinition.StartsWith("System.", StringComparison.Ordinal) && typeDefinition.Count(x => x == '.') == 1)
        {
            return typeDefinition.Substring("System.".Length);
        }
        return typeDefinition;
    }

    private static string GetTypeDefinition(Type type)
    {
        if (type.IsGenericType)
        {
            var typeDefinition = type.GetGenericTypeDefinition();
            var genericArguments = string.Join(",", type.GetGenericArguments().Select(x => $"{GetTypeDefinition(x)}"));
            return $"{(typeDefinition.FullName ?? typeDefinition.Name).Split('`')[0]}<{genericArguments}>";
        }
        return $"{(type.FullName ?? type.Name).Split('`')[0]}";
    }

}
