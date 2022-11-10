using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlainlyIpc.SourceGenerator;

[Generator]
public class ProxyClassCreator : ISourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> RequestingClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case ClassDeclarationSyntax classDeclarationSyntax:
                    if (classDeclarationSyntax.AttributeLists.Any())
                    {
                        RequestingClasses.Add(classDeclarationSyntax);
                    }
                    break;
                case InterfaceDeclarationSyntax interfaceDeclarationSyntax:
                    if (interfaceDeclarationSyntax.AttributeLists.Any())
                    {
                        RequestingClasses.Add(interfaceDeclarationSyntax);
                    }
                    break;
            }
            //var requiresGeneration = interfaceDeclarationSyntax.AttributeLists
            //    .SelectMany(x => x.Attributes)
            //    .Select(x => x.Name)
            //    .OfType<IdentifierNameSyntax>()
            //    .Any(x => x.Identifier.ValueText == "GenerateProxyAttribute" || x.Identifier.ValueText == "GenerateProxy");
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver
            || !receiver.RequestingClasses.Any()) { return; }

        foreach (TypeDeclarationSyntax declarationSyntax in receiver.RequestingClasses)
        {
            var list = declarationSyntax.AttributeLists.SelectMany(x => x.Attributes).Select(x => x.Name).ToList();
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(declarationSyntax.SyntaxTree);
            INamedTypeSymbol? namedTypeSymbol = semanticModel.GetDeclaredSymbol(declarationSyntax);

            if (namedTypeSymbol is null) { continue; }
            if (!namedTypeSymbol.GetAttributes().Select(x => x.AttributeClass?.Name).Any(x => x == "GenerateProxyAttribute" || x == "GenerateProxy")) { continue; }
            if (namedTypeSymbol.IsStatic) { continue; }

            var (className, sourceCode) = GenerateSource(namedTypeSymbol, declarationSyntax is not InterfaceDeclarationSyntax);

            context.AddSource($"{className}.g.cs", sourceCode);
        }
    }

    private static (string ClassName, string SourceCode) GenerateSource(INamedTypeSymbol namedTypeSymbol, bool partialClassModel)
    {
        string fullName = GetFullName(namedTypeSymbol, "");
        var fullNamespace = fullName.Substring(0, fullName.LastIndexOf('.'));
        INamedTypeSymbol interfaceTypeSymbol = partialClassModel ? namedTypeSymbol.Interfaces.First() : namedTypeSymbol;
        var interfaceTypeName = GetFullName(interfaceTypeSymbol, "");
        string className;
        if (partialClassModel)
        {
            className = namedTypeSymbol.Name;
        }
        else
        {
            className = $"{interfaceTypeSymbol.Name}RemoteProxy";
            if (className.StartsWith("I", StringComparison.Ordinal)) { className = className.Substring(1); }
        }
        RemoteProxyClassBuilder builder = new(fullNamespace, className, interfaceTypeName, partialClassModel);
        var methods = interfaceTypeSymbol.GetMembers().OfType<IMethodSymbol>();
        foreach (var method in methods)
        {
            if (method.IsStatic) { continue; }
            var returnType = GetFullName(method.ReturnType, interfaceTypeName);
            var generics = method.TypeArguments.Select(x => GetFullName(x, interfaceTypeName)).ToArray();
            var parameters = method.Parameters.Select(x => (Type: GetFullName(x.Type, interfaceTypeName), x.Name, x.IsParams)).ToArray();
            builder.AddRemoteCall(method.Name, returnType, generics, parameters);
        }
        return (className, builder.ToString());
    }

    private static string GetGenerics(IEnumerable<ITypeSymbol> typeParameters, string baseNameSpace)
    {
        if (typeParameters.Any())
        {
            string prms = string.Join(", ", typeParameters.Select(x => GetFullName(x, baseNameSpace)));
            return $"<{prms}>";
        }
        return "";
    }

    private static string GetFullName(ISymbol symbol, string baseNameSpace)
    {
        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            return GetFullName(arrayTypeSymbol.ElementType, baseNameSpace) + "[]";
        }
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
            generics = GetGenerics(typeParameterSymbol.TypeArguments, baseNameSpace);
        }
        if (nss.Any())
        {
            var nsText = string.Join(".", nss);
            if (nsText == baseNameSpace)
            {
                return symbol.Name + generics;
            }
            return $"{nsText}.{symbol.Name}{generics}";
        }
        return symbol.Name + generics;
    }

}

