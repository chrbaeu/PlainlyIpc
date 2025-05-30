﻿using System.Reflection;
using System.Text.RegularExpressions;

namespace PlainlyIpc.Internal;

internal static class TypeExtensions
{
    private static readonly Regex typeStringRegex = new("^([^ ]*) (([^ \\[]*)(\\[\\])*)(\\[(.*)\\]){0,1}$", RegexOptions.Compiled);

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
            var assemblyFilterString = match.Groups[1].Value + ",";
            bool predicate(Assembly x) => x.FullName?.StartsWith(assemblyFilterString, StringComparison.Ordinal) ?? false;
            var assembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), predicate);
            if (assembly is null)
            {
                var assemblies = GetAssemblies();
                var refAssembly = assemblies.Find(predicate);
                assembly = AppDomain.CurrentDomain.Load(refAssembly?.FullName ?? match.Groups[1].Value);
            }
            type = assembly?.GetType(match.Groups[2].Value);
        }
        if (type is not null && !string.IsNullOrEmpty(match.Groups[6].Value))
        {
            var genericArguments = SplitGenericTypeArguments(match.Groups[6].Value);
            type = type.MakeGenericType(genericArguments.Select(GetTypeFromTypeString).ToArray());
        }
        return type ?? throw new ArgumentException("Invalid type info!", nameof(typeInfo));
    }

    private static List<Assembly> GetAssemblies()
    {
        var returnAssemblies = new List<Assembly>();
        var loadedAssemblies = new HashSet<string>();
        var assembliesToCheck = new Queue<Assembly>();
        assembliesToCheck.Enqueue(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
        while (assembliesToCheck.Count > 0)
        {
            var assemblyToCheck = assembliesToCheck.Dequeue();
            foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
            {
                if (!loadedAssemblies.Contains(reference.FullName))
                {
                    var assembly = Assembly.Load(reference);
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    returnAssemblies.Add(assembly);
                }
            }
        }
        return returnAssemblies;
    }

    private static List<string> SplitGenericTypeArguments(string genericArguments)
    {
        List<string> typeArguments = [];
        int markers = 0, lastIndex = 0;
        for (var i = 0; i < genericArguments.Length; i++)
        {
            var c = genericArguments[i];
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
