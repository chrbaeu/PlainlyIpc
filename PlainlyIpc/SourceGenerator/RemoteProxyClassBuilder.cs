namespace PlainlyIpc.SourceGenerator;

internal class RemoteProxyClassBuilder
{
    private readonly string fullNamespace;
    private readonly string className;
    private readonly string interfaceType;
    private readonly bool isPartial;
    private readonly string accessibility;
    private readonly List<string> usings = [];
    private readonly List<string> methods = [];

    public RemoteProxyClassBuilder(string fullNamespace, string className, string interfaceType, bool isPartial, string accessibility)
    {
        this.fullNamespace = fullNamespace;
        this.className = className;
        var lastDotIndex = interfaceType.LastIndexOf('.');
        var interfaceNamespace = interfaceType.Substring(0, lastDotIndex);
        this.interfaceType = interfaceType.Substring(lastDotIndex + 1);
        this.isPartial = isPartial;
        this.accessibility = accessibility;
        usings.Add("System");
        usings.Add("System.Threading.Tasks");
        usings.Add("PlainlyIpc.Interfaces");
        usings.Add(interfaceNamespace);
    }

    public void AddRemoteCall(string methodName, string returnType, IReadOnlyList<string> generics, IReadOnlyList<(string Type, string Name, bool IsParams)> parameters)
    {
        const string fullTaskName = "System.Threading.Tasks.Task";
        if (returnType == "System.Void") { returnType = "void"; }
        string unpackedReturnType = returnType;
        bool isAsync = false;
#if NETSTANDARD2_0
        returnType = returnType.Replace("System.Threading.Tasks.ValueTask", fullTaskName);
#else
        returnType = returnType.Replace("System.Threading.Tasks.ValueTask", fullTaskName, StringComparison.Ordinal);
#endif
        if (returnType.StartsWith(fullTaskName, StringComparison.OrdinalIgnoreCase))
        {
            isAsync = true;
            if (returnType == fullTaskName)
            {
                unpackedReturnType = "void";
            }
            else
            {
                unpackedReturnType = returnType.Substring(28, returnType.Length - 29);
            }
        }
        else
        {
            returnType = returnType == "void" ? fullTaskName : $"{fullTaskName}<{returnType}>";
        }
        returnType = TrimNamespace(returnType);
        unpackedReturnType = TrimNamespace(unpackedReturnType);
        bool hasReturnValue = unpackedReturnType != "void";
        string returnStatement = hasReturnValue ? "return " : "";
        string paramDefs = string.Join(", ", parameters.Select(x => $"{(x.IsParams ? "params " : "")}{TrimNamespace(x.Type)} {x.Name}"));
        string paramVals = string.Join(", ", parameters.Select(x => x.Name));
        string genericsStatement = generics.Any() ? $"<{string.Join(", ", generics.Select(TrimNamespace))}>" : ""; ;
        if (isAsync)
        {
            methods.Add($$"""
                    public async {{returnType}} {{methodName}}{{genericsStatement}}({{paramDefs}})
                    {
                        {{(hasReturnValue ? "return " : "")}}await ipcHandler.ExecuteRemote<{{interfaceType}}{{(hasReturnValue ? $", {unpackedReturnType}" : "")}}>(
                            plainlyRpcService => plainlyRpcService.{{methodName}}{{genericsStatement}}({{paramVals}})).ConfigureAwait(false);
                    }
            """);
        }
        else
        {
            methods.Add($$"""
                    [Obsolete("Remote calls are asynchronous, use the asynchronous version!")]
                    public {{unpackedReturnType}} {{methodName}}{{genericsStatement}}({{paramDefs}})
                    {
                        {{(hasReturnValue ? "return " : "")}}Task.Run(() => {{methodName}}Async{{genericsStatement}}({{paramVals}})).GetAwaiter().GetResult();
                    }
            """);
            methods.Add($$"""
                    public async {{returnType}} {{methodName}}Async{{genericsStatement}}({{paramDefs}})
                    {
                        {{(hasReturnValue ? "return " : "")}}await ipcHandler.ExecuteRemote<{{interfaceType}}{{(hasReturnValue ? $", {unpackedReturnType}" : "")}}>(
                            plainlyRpcService => plainlyRpcService.{{methodName}}{{genericsStatement}}({{paramVals}})).ConfigureAwait(false);
                    }
            """);
        }
    }

    public override string ToString()
    {
        return $$"""
            {{string.Join("\n", usings.Select(x => $"using {x};"))}}

            namespace {{fullNamespace}} {

                {{accessibility}} {{(isPartial ? "partial" : "sealed")}} class {{className}}{{(interfaceType is null ? "" : $" : {interfaceType}")}}
                {

                    private readonly IIpcHandler ipcHandler;
                
                    public {{className}}(IIpcHandler ipcHandler)
                    {
                        this.ipcHandler = ipcHandler;
                    }

            {{string.Join("\n" + "\n", methods)}}

                }

            }
            """;
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

}
