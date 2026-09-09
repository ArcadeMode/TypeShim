using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal class JSObjectMethodResolver(List<InteropTypeInfo> resolvedTypes)
{
    internal string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
    {
        string extensionMethodName = new JSObjectExtensionInfo(typeInfo).GetGetPropertyAsMethodName();
        resolvedTypes.Add(typeInfo);
        return extensionMethodName;
    }
}