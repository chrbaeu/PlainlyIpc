using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlainlyIpc.SourceGenerator;

[Generator]
public class InterfaceProxyCreator : ISourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InterfaceDeclarationSyntax> DecoratorRequestingInterfaces { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not InterfaceDeclarationSyntax interfaceDeclarationSyntax) { return; }
            if (!interfaceDeclarationSyntax.AttributeLists.Any()) { return; }
            //var requiresGeneration = interfaceDeclarationSyntax.AttributeLists
            //    .SelectMany(x => x.Attributes)
            //    .Select(x => x.Name)
            //    .OfType<IdentifierNameSyntax>()
            //    .Any(x => x.Identifier.ValueText == "GenerateProxyAttribute" || x.Identifier.ValueText == "GenerateProxy");
            //if (requiresGeneration)
            {
                DecoratorRequestingInterfaces.Add(interfaceDeclarationSyntax);
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static void GetAllTypeSymbolsRecursive(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> types)
    {
        foreach (var namespaze in namespaceSymbol.GetNamespaceMembers())
        {
            types.AddRange(namespaze.GetTypeMembers());
            foreach (var type in namespaze.GetTypeMembers())
            {
                types.AddRange(type.GetTypeMembers());
            }
            GetAllTypeSymbolsRecursive(namespaze, types);
        }
    }


    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver
            || !receiver.DecoratorRequestingInterfaces.Any()) { return; }

        foreach (var interfaceDeclarationSyntax in receiver.DecoratorRequestingInterfaces)
        {
            var list = interfaceDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name).ToList();
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(interfaceDeclarationSyntax.SyntaxTree);
            INamedTypeSymbol? namedTypeSymbol = semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax);

            if (namedTypeSymbol is null) { continue; }
            if (!namedTypeSymbol.GetAttributes().Select(x => x.AttributeClass?.Name).Any(x => x == "GenerateProxyAttribute" || x == "GenerateProxy")) { continue; }
            if (namedTypeSymbol.IsStatic) { continue; }

            var fullName = GetFullName(namedTypeSymbol);
            var interfaceName = namedTypeSymbol.Name;

            var methods = namedTypeSymbol.GetMembers().OfType<IMethodSymbol>();
            List<string> methodDefinitions = new();
            foreach (var method in methods)
            {
                if (method.IsStatic) { continue; }
                var returnType = GetFullName(method.ReturnType);
                if (!returnType.StartsWith("System.Threading.Tasks.Task"))
                {
                    methodDefinitions.Add($"// Return type '{returnType}' is not supported!");
                    continue;
                }
                var returnStatement = returnType == "System.Threading.Tasks.Task" ? "" : "return ";
                var remoteReturnStatement = returnType.StartsWith("System.Threading.Tasks.Task<", StringComparison.OrdinalIgnoreCase)
                    ? ", " + returnType.Substring(28, returnType.Length - 29)
                    : returnType == "System.Threading.Tasks.Task" ? "" : ", " + returnType;
                var parameters = method.Parameters.Select(x => (Type: GetFullName(x.Type), Name: x.Name, IsParam: x.IsParams)).ToArray();
                string paramDefs = string.Join(", ", parameters.Select(x => $"{(x.IsParam ? "params " : "")}{x.Type} {x.Name}"));
                string paramVals = string.Join(", ", parameters.Select(x => x.Name));
                string generics = GetGenerics(method.TypeArguments);
                methodDefinitions.Add($$"""
                        public async {{returnType}} {{method.Name}}{{generics}}({{paramDefs}})
                        {
                            {{returnStatement}}await ipcHandler.ExecuteRemote<{{fullName}}{{remoteReturnStatement}}>(plainlyRpcService
                                => plainlyRpcService.{{method.Name}}{{generics}}({{paramVals}}));
                        }
                """);
            }

            var interfaceStatement = "";
            if (!methodDefinitions.Any(x => x.StartsWith("//"))) { interfaceStatement = $" : {fullName}"; }

            var className = $"{interfaceName}RemoteProxy";
            if (className.StartsWith("I", StringComparison.Ordinal)) { className = className.Substring(1); }

            var fullNamespace = fullName.Substring(0, fullName.LastIndexOf('.'));

            var sourceCode = $$"""
                using System;
                using System.Threading.Tasks;
                using PlainlyIpc.Interfaces;

                namespace {{fullNamespace}} {

                    public sealed class {{className}}{{interfaceStatement}}
                    {

                        private readonly IIpcHandler ipcHandler;
                
                        public {{className}}(IIpcHandler ipcHandler)
                        {
                            this.ipcHandler = ipcHandler;
                        }

                {{string.Join("\n", methodDefinitions)}}

                    }

                }
                """;

            context.AddSource($"{className}.g.cs", sourceCode);
        }
    }

    public static string GetGenerics(IEnumerable<ITypeSymbol> typeParameters)
    {
        if (typeParameters.Any())
        {
            string prms = string.Join(", ", typeParameters.Select(x => GetFullName(x)));
            return $"<{prms}>";
        }
        return "";
    }

    internal static string GetFullName(ISymbol symbol)
    {
        var nss = new List<string>();
        INamespaceSymbol ns;

        if (symbol.ContainingType != null)
        {
            nss.Add(symbol.ContainingType.Name);
            ns = symbol.ContainingType.ContainingNamespace;
        }
        else
        {
            ns = symbol.ContainingNamespace;
        }

        while (ns != null)
        {
            if (string.IsNullOrWhiteSpace(ns.Name))
            {
                break;
            }
            nss.Add(ns.Name);
            ns = ns.ContainingNamespace;
        }
        nss.Reverse();
        var generics = "";
        if (symbol is INamedTypeSymbol typeParameterSymbol)
        {
            generics = GetGenerics(typeParameterSymbol.TypeArguments);
        }
        if (nss.Any())
        {
            return $"{string.Join(".", nss)}.{symbol.Name}{generics}";
        }
        return symbol.Name + generics;
    }


    //public void Execute(GeneratorExecutionContext context)
    //{
    //    IEnumerable<SyntaxNode> allNodes = context.Compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
    //    IEnumerable<InterfaceDeclarationSyntax> allInterfaces = allNodes
    //        .Where(d => d.IsKind(SyntaxKind.InterfaceDeclaration))
    //        .OfType<InterfaceDeclarationSyntax>();
    //    foreach (var classDeclaration in allInterfaces)
    //    {
    //        var attribute = classDeclaration.AttributeLists
    //            .SelectMany(x => x.Attributes)
    //            .Where(attr => attr.Name.ToString() == "GenerateProxyAttribute")
    //            .FirstOrDefault();

    //        if (attribute is not null)
    //        {
    //            var ns = GetNamespace(classDeclaration);
    //            classDeclaration.n
    //        }
    //    }

    //    var source = """
    //        // <auto-generated/>
    //        namespace PlainlyIpc.SourceGenerator;
    //        /// <summary>
    //        /// Attribute for automatic generation of proxy classes.
    //        /// </summary>
    //        [AttributeUsage(AttributeTargets.Interface)]
    //        public sealed class GenerateProxyAttribute : Attribute
    //        {
    //        }            
    //        """;
    //    context.AddSource($"{"GenerateProxyAttribute"}.g.cs", source);
    //}

    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        string nameSpace = string.Empty;
        SyntaxNode? potentialNamespaceParent = syntax.Parent;
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }
        return nameSpace;
    }

}

