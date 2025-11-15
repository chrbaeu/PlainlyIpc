using Microsoft.CodeAnalysis;
using System.Threading;

namespace PlainlyIpc.SourceGenerator;

[Generator]
public class ProxyClassCreator : IIncrementalGenerator
{
    private sealed record ProxyMethodDefinition(string Name, string ReturnType, IReadOnlyList<string> Generics,
        IReadOnlyList<(string Type, string Name, bool IsParams)> Parameters);

    private sealed record ProxyClassDefinition(string FullNamespace, string InterfaceTypeName, string ClassName,
        string Accessibility, bool IsPartialClass, IReadOnlyList<ProxyMethodDefinition> Methods);

    private static readonly SymbolDisplayFormat fullyQualifiedFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            var source = """
                using System;

                namespace PlainlyIpc.SourceGenerator
                {
                    /// <summary>
                    /// Attribute for automatic generation of proxy classes.
                    /// </summary>
                    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
                    public sealed class GenerateProxyAttribute : Attribute
                    {
                    }
                }
                """;
            ctx.AddSource("GenerateProxyAttribute.g.cs", source);
        });

        var proxyTargets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "PlainlyIpc.SourceGenerator.GenerateProxyAttribute",
                Predicate,
                Transform)
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        context.RegisterSourceOutput(proxyTargets, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token) => true;

    private static ProxyClassDefinition? Transform(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken token)
    {
        if (syntaxContext.TargetSymbol is not INamedTypeSymbol namedTypeSymbol) { return null; }
        if (namedTypeSymbol.IsStatic) { return null; }
        var isClass = namedTypeSymbol.TypeKind == TypeKind.Class;
        var isInterface = namedTypeSymbol.TypeKind == TypeKind.Interface;
        if (!isClass && !isInterface) { return null; }
        if (isClass && namedTypeSymbol.Interfaces.Length == 0) { return null; }

        var fullTypeName = GetFullTypeString(namedTypeSymbol);
        var lastDot = fullTypeName.LastIndexOf('.');
        var fullNamespace = lastDot >= 0 ? fullTypeName[..lastDot] : string.Empty;
        var interfaceTypeSymbol = isClass ? namedTypeSymbol.Interfaces.First() : namedTypeSymbol;
        var interfaceTypeName = GetFullTypeString(interfaceTypeSymbol);
        string className;
        if (isClass)
        {
            className = namedTypeSymbol.Name;
        }
        else
        {
            className = $"{interfaceTypeSymbol.Name}RemoteProxy";
            if (className.StartsWith("I", StringComparison.Ordinal))
            {
                className = className[1..];
            }
        }
        var accessibility = namedTypeSymbol.DeclaredAccessibility switch
        {
            Accessibility.NotApplicable => "private",
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "protected",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected",
            Accessibility.Public => "public",
            _ => "private"
        };
        List<ProxyMethodDefinition> proxyMethods = [];
        foreach (var methodSymbol in interfaceTypeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (methodSymbol.IsStatic) { continue; }
            var returnType = GetFullTypeString(methodSymbol.ReturnType);
            var generics = methodSymbol.TypeArguments.Select(GetFullTypeString).ToArray();
            var parameters = methodSymbol.Parameters.Select(x => (Type: GetFullTypeString(x.Type), x.Name, x.IsParams)).ToArray();
            proxyMethods.Add(new(methodSymbol.Name, returnType, generics, parameters));
        }
        return new ProxyClassDefinition(fullNamespace, interfaceTypeName, className, accessibility, isClass, proxyMethods);
    }

    private static void Generate(SourceProductionContext context, ProxyClassDefinition proxyTarget)
    {
        var builder = new RemoteProxyClassBuilder(proxyTarget.FullNamespace, proxyTarget.ClassName,
            proxyTarget.InterfaceTypeName, proxyTarget.IsPartialClass, proxyTarget.Accessibility);
        foreach (var method in proxyTarget.Methods)
        {
            builder.AddRemoteCall(method.Name, method.ReturnType, method.Generics, method.Parameters);
        }
        context.AddSource($"{proxyTarget.ClassName}.g.cs", builder.ToString());
    }

    private static string GetFullTypeString(ISymbol symbol) => symbol.ToDisplayString(fullyQualifiedFormat);
}
