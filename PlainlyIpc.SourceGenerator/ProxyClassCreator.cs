using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlainlyIpc.SourceGenerator;

[Generator]
public class ProxyClassCreator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat fullyQualifiedFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register attribute source
        context.RegisterPostInitializationOutput(ctx =>
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
            ctx.AddSource("GenerateProxyAttribute.g.cs", source);
        });

        // Register syntax provider for types with attributes
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is TypeDeclarationSyntax tds && tds.AttributeLists.Count > 0,
                transform: (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
            .Where(tds => tds != null);

        var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndTypes, (spc, source) =>
        {
            var (compilation, typeDecls) = source;
            foreach (var declarationSyntax in typeDecls)
            {
                var semanticModel = compilation.GetSemanticModel(declarationSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(declarationSyntax, spc.CancellationToken) is not INamedTypeSymbol namedTypeSymbol) continue;
                if (!namedTypeSymbol.GetAttributes().Select(x => x.AttributeClass?.Name).Any(x => x == "GenerateProxyAttribute" || x == "GenerateProxy")) continue;
                if (namedTypeSymbol.IsStatic) continue;
                var (className, sourceCode) = ProxyClassCreator.GenerateSourceCode(namedTypeSymbol, declarationSyntax is not InterfaceDeclarationSyntax);
                spc.AddSource($"{className}.g.cs", sourceCode);
            }
        });
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
        string accessibility = namedTypeSymbol.DeclaredAccessibility switch
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
        RemoteProxyClassBuilder builder = new(fullNamespace, className, interfaceTypeName, partialClassModel, accessibility);
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
}
