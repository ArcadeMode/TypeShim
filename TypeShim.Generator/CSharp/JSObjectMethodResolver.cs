using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal class JSObjectMethodResolver(List<InteropTypeInfo> resolvedTypes)
{
    internal string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
    {
        if (typeInfo.ManagedType is KnownManagedType.Nullable)
        {
            return ResolveJSObjectMethodName(typeInfo.TypeArgument!);
        }

        string extensionMethodName = new JSObjectExtensionInfo(typeInfo).GetGetPropertyAsMethodName();
        resolvedTypes.Add(typeInfo);
        return extensionMethodName;
    }

    internal IEnumerable<InteropTypeInfo> GetResolvedTypes()
    {
        return resolvedTypes;
    }
}