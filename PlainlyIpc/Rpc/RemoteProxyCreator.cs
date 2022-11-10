using PlainlyIpc.SourceGenerator;
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
    /// <param name="fullNamespace">"The namespace."</param>
    public static void CreateProxyClass<TInterface>(string outputFolderPath, string fullNamespace)
    {
        var src = CreateProxyClass<TInterface>(fullNamespace);
        File.WriteAllText(Path.Combine(Path.GetFullPath(outputFolderPath), GetProxyClassName<TInterface>() + ".cs"), src);
    }

    /// <summary>
    /// Creates the source code for a proxy classes for the RPC interfaces. 
    /// </summary>
    /// <typeparam name="TInterface">The RPC interface.</typeparam>
    /// <param name="fullNamespace">"The namespace."</param>
    /// <param name="asPartialClass">"Flag that indicates if a partial class should be created."</param>
    /// <returns>The source code for the proxy class.</returns>
    public static string CreateProxyClass<TInterface>(string fullNamespace, bool asPartialClass = false)
    {
        var interfaceType = typeof(TInterface);
        var className = GetProxyClassName<TInterface>();
        RemoteProxyClassBuilder builder = new(fullNamespace, className, GetTypeDefinition(interfaceType), asPartialClass);
        foreach (var method in interfaceType.GetMethods())
        {
            string methodName = method.Name;
            var generics = method.GetGenericArguments().Select(GetTypeDefinition).ToArray();
            var parameters = method.GetParameters()
                .Select(x => (GetTypeDefinition(x.ParameterType), x.Name ?? "", x.IsDefined(typeof(ParamArrayAttribute), false)))
                .ToArray();
            var returnType = GetTypeDefinition(method.ReturnType);
            if (returnType == "System.Void") { returnType = "void"; }
            builder.AddRemoteCall(methodName, returnType, generics, parameters);
        }
        return builder.ToString();
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
