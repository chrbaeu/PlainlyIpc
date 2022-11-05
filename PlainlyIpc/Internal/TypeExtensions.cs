using System.Text.RegularExpressions;

namespace PlainlyIpc.Internal;

internal static class TypeExtensions
{
    // https://regex101.com/r/7OhC2d/2
    private static readonly Regex typeStringRegex = new(@"^([^ ]*) ([^ \[]*)(\[(.*)\]){0,1}$", RegexOptions.Compiled);

    public static string GetTypeString(this Type type)
    {
        if (type.IsGenericType)
        {
            var typeDefinition = type.GetGenericTypeDefinition();
            var genericArguments = string.Join(",", type.GetGenericArguments().Select(x => $"[{x.GetTypeString()}]"));
            return $"{typeDefinition.Assembly.GetName().Name} {typeDefinition.FullName}[{genericArguments}]";
        }
        return $"{type.Assembly.GetName().Name} {type.FullName}";
    }

    public static Type GetTypeFromTypeString(string typeInfo)
    {
        if (string.IsNullOrWhiteSpace(typeInfo)) { throw new ArgumentException("Invalid type info!", nameof(typeInfo)); }
        var match = typeStringRegex.Match(typeInfo);
        if (!match.Success && match.Groups.Count >= 3 && match.Groups.Count <= 5) { throw new ArgumentException("Invalid type info!", nameof(typeInfo)); }
        var type = Type.GetType(match.Groups[2].Value);
        if (type is null)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == match.Groups[1].Value);
            if (assembly is null)
            {
                assembly = AppDomain.CurrentDomain.Load(match.Groups[1].Value);
            }
            type = assembly?.GetType(match.Groups[2].Value);
        }
        if (type is not null && !string.IsNullOrEmpty(match.Groups[4].Value))
        {
            var genericArguments = SplitGenericTypeArguments(match.Groups[4].Value);
            type = type.MakeGenericType(genericArguments.Select(x => GetTypeFromTypeString(x)).ToArray());
        }
        return type ?? throw new ArgumentException("Invalid type info!", nameof(typeInfo));
    }

    private static List<string> SplitGenericTypeArguments(string genericArguments)
    {
        List<string> typeArguments = new();
        int markers = 0, lastIndex = 0;
        for (int i = 0; i < genericArguments.Length; i++)
        {
            char c = genericArguments[i];
            if (c == '[') { markers++; }
            if (c == ']') { markers--; }
            if (c == ',' && markers == 0)
            {
                typeArguments.Add(genericArguments.Substring(lastIndex + 1, i - lastIndex - 2));
                lastIndex = i + 1;
            }
        }
        typeArguments.Add(genericArguments.Substring(lastIndex + 1, genericArguments.Length - lastIndex - 2));
        return typeArguments;
    }

}
