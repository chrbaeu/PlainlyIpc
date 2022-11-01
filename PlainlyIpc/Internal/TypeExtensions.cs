namespace PlainlyIpc.Internal;

internal static class TypeExtensions
{
    public static string GetTypeString(this Type type)
    {
        return $"{type.Assembly.GetName().Name} {type.FullName}";
    }

    public static Type GetTypeFromTypeString(string typeInfo)
    {
        if (string.IsNullOrWhiteSpace(typeInfo)) { throw new ArgumentException("Invalid type info!", nameof(typeInfo)); }
        var parts = typeInfo.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) { throw new ArgumentException("Invalid type info!", nameof(typeInfo)); }
        var type = Type.GetType(parts[1]);
        if (type is null)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == parts[0]);
            if (assembly is null)
            {
                assembly = AppDomain.CurrentDomain.Load(parts[0]);
            }
            type = assembly?.GetType(parts[1]);
        }
        return type ?? throw new ArgumentException("Invalid type info!", nameof(typeInfo));
    }

}
