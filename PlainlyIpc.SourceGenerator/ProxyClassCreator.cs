using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlainlyIpc.SourceGenerator;

[Generator]
public class ProxyClassCreator : ISourceGenerator
{
    private static SymbolDisplayFormat fullyQualifiedFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );

    public void Initialize(GeneratorInitializationContext context)
    {
        var source = """
                using System;

                namespace PlainlyIpc.SourceGenerator
                {
                    /// <summary>
                    /// Attribute for automatic generation of proxy classes.
                    /// </summary>
                    [AttributeUsage(AttributeTargets.Interface|AttributeTargets.Class)]
                    public sealed class GenerateProxyAttribute : Attribute
                    {
                    }
                }
                """;
        context.RegisterForPostInitialization((i) => i.AddSource("GenerateProxyAttribute.g.cs", source));
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver || receiver.TypesToInspect.Count == 0) { return; }

        foreach (TypeDeclarationSyntax declarationSyntax in receiver.TypesToInspect)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(declarationSyntax.SyntaxTree);
            INamedTypeSymbol? namedTypeSymbol = semanticModel.GetDeclaredSymbol(declarationSyntax, context.CancellationToken);

            if (namedTypeSymbol is null) { continue; }
            if (!namedTypeSymbol.GetAttributes().Select(x => x.AttributeClass?.Name).Any(x => x == "GenerateProxyAttribute" || x == "GenerateProxy")) { continue; }
            if (namedTypeSymbol.IsStatic) { continue; }

            var (className, sourceCode) = GenerateSourceCode(namedTypeSymbol, declarationSyntax is not InterfaceDeclarationSyntax);

            context.AddSource($"{className}.g.cs", sourceCode);
        }
    }

    private static (string ClassName, string SourceCode) GenerateSourceCode(INamedTypeSymbol namedTypeSymbol, bool partialClassModel)
    {
        string fullName = GetFullTypeString(namedTypeSymbol);
        var fullNamespace = fullName.Substring(0, fullName.LastIndexOf('.'));
        INamedTypeSymbol interfaceTypeSymbol = partialClassModel ? namedTypeSymbol.Interfaces.First() : namedTypeSymbol;
        var interfaceTypeName = GetFullTypeString(interfaceTypeSymbol);
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
            var returnType = GetFullTypeString(method.ReturnType);
            var generics = method.TypeArguments.Select(GetFullTypeString).ToArray();
            var parameters = method.Parameters.Select(x => (Type: GetFullTypeString(x.Type), x.Name, x.IsParams)).ToArray();
            builder.AddRemoteCall(method.Name, returnType, generics, parameters);
        }
        return (className, builder.ToString());
    }

    private static string GetFullTypeString(ISymbol symbol) => symbol.ToDisplayString(fullyQualifiedFormat);

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> TypesToInspect { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not TypeDeclarationSyntax typeDeclarationSyntax) { return; }
            if (typeDeclarationSyntax.AttributeLists.Count == 0) { return; }
            switch (syntaxNode)
            {
                case ClassDeclarationSyntax classDeclarationSyntax:
                    TypesToInspect.Add(classDeclarationSyntax);
                    break;
                case InterfaceDeclarationSyntax interfaceDeclarationSyntax:
                    TypesToInspect.Add(interfaceDeclarationSyntax);
                    break;
            }
        }
    }

}
