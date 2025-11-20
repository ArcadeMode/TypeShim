namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypeScriptMethodRenderer(TypeScriptTypeMapper typeMapper)
{
    internal string RenderMethodSignature(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}({RenderMethodParameters(methodInfo, includeInstanceParameter: true)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnKnownType, methodInfo.ReturnCLRTypeSyntax.ToString())}";
    }

    private string RenderMethodParameters(MethodInfo methodInfo, bool includeInstanceParameter)
    {
        return string.Join(", ", methodInfo.MethodParameters
            .Select(p => $"{p.ParameterName}: {typeMapper.ToTypeScriptType(p.KnownType, p.CLRTypeSyntax.ToString())}"));
    }

    /// <summary>
    /// Renders only the parameter names for method call, with the same names as defined in the method info.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    internal string RenderMethodCallParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p => p.ParameterName));
    }

    /// <summary>
    /// Renders only the parameter names for method call, with the same names as defined in the method info.
    /// Excludes the injected instance parameter if present.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    internal string RenderMethodCallParametersWithInstanceParameterExpression(MethodInfo methodInfo, string instanceParameterExpression)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p => p.IsInjectedInstanceParameter ? instanceParameterExpression : p.ParameterName));
    }
}