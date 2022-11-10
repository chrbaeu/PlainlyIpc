namespace PlainlyIpc.SourceGenerator;

internal class RemoteProxyClassBuilder
{
    private readonly string fullNamespace;
    private readonly string className;
    private readonly string interfaceType;
    private readonly bool isPartial;
    private readonly List<string> usings = new();
    private readonly List<string> methods = new();

    public RemoteProxyClassBuilder(string fullNamespace, string className, string interfaceType, bool isPartial)
    {
        this.fullNamespace = fullNamespace;
        this.className = className;
        var lastDotIndex = interfaceType.LastIndexOf('.');
        var interfaceNamespace = interfaceType.Substring(0, lastDotIndex);
        this.interfaceType = interfaceType.Substring(lastDotIndex + 1);
        this.isPartial = isPartial;
        usings.Add("System");
        usings.Add("System.Threading.Tasks");
        usings.Add("PlainlyIpc.Interfaces");
        usings.Add(interfaceNamespace);
    }

    public void AddRemoteCall(string methodName, string returnType, IList<string> generics, IList<(string Type, string Name, bool IsParams)> parameters)
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
                            plainlyRpcService => plainlyRpcService.{{methodName}}{{genericsStatement}}({{paramVals}}));
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
                            plainlyRpcService => plainlyRpcService.{{methodName}}{{genericsStatement}}({{paramVals}}));
                    }
            """);
        }
    }

    public override string ToString()
    {
        return $$"""
            {{string.Join(Environment.NewLine, usings.Select(x => $"using {x};"))}}

            namespace {{fullNamespace}} {

                public {{(isPartial ? "partial" : "sealed")}} class {{className}}{{(interfaceType is null ? "" : $" : {interfaceType}")}}
                {

                    private readonly IIpcHandler ipcHandler;
                
                    public {{className}}(IIpcHandler ipcHandler)
                    {
                        this.ipcHandler = ipcHandler;
                    }

            {{string.Join(Environment.NewLine + Environment.NewLine, methods)}}

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
