namespace TypeShim.Generator.Typescript;

internal class TypeScriptMethodRenderer(TypescriptSymbolNameProvider symbolNameProvider)
{
    internal string RenderProxyPropertyGetterSignature(MethodInfo methodInfo)
    {
        string returnType = symbolNameProvider.GetProxyReferenceNameIfExists(methodInfo.ReturnType) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
        return $"get {methodInfo.Name.TrimStart("get_")}({RenderProxyMethodParameters(methodInfo)}): {returnType}";
    }

    internal string RenderProxyPropertySetterSignature(MethodInfo methodInfo)
    {
        return $"set {methodInfo.Name.TrimStart("set_")}({RenderProxyMethodParameters(methodInfo)})";
    }

    internal string RenderProxyMethodSignature(MethodInfo methodInfo)
    {
        string returnType = symbolNameProvider.GetProxyReferenceNameIfExists(methodInfo.ReturnType) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);

        string optionalAsync = methodInfo.ReturnType.IsTaskType ? "async " : string.Empty;
        return $"{optionalAsync}{methodInfo.Name}({RenderProxyMethodParameters(methodInfo)}): {returnType}";
    }

    private string RenderProxyMethodParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p =>
        {
            string returnType = symbolNameProvider.GetProxySnapshotUnionIfExists(p.Type) ?? symbolNameProvider.GetNakedSymbolReference(p.Type);
            return $"{p.Name}: {returnType}";
        }));
    }

    internal string RenderInteropMethodSignature(MethodInfo methodInfo, MethodOverloadInfo? overloadInfo = null)
    {
        string returnType = symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
        return $"{overloadInfo?.Name ?? methodInfo.Name}({RenderInteropMethodParameters(overloadInfo?.MethodParameters ?? methodInfo.MethodParameters)}): {returnType}";

        string RenderInteropMethodParameters(IEnumerable<MethodParameterInfo> parameterInfos)
        {
            return string.Join(", ", parameterInfos.Select(p => $"{p.Name}: {symbolNameProvider.GetNakedSymbolReference(p.Type)}"));
        }
    }
}