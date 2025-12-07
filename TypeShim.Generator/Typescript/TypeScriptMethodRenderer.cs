using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptMethodRenderer(TypeScriptTypeMapper typeMapper)
{
    internal string RenderPropertyGetterSignatureForClass(MethodInfo methodInfo)
    {

        return $"get {methodInfo.Name.TrimStart("get_")}({RenderMethodParameters(methodInfo)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnType.ManagedType, methodInfo.ReturnType.CLRTypeSyntax)}";
    }

    internal string RenderPropertySetterSignatureForClass(MethodInfo methodInfo)
    {
        return $"set {methodInfo.Name.TrimStart("set_")}({RenderMethodParameters(methodInfo)})";
    }

    internal string RenderMethodSignatureForClass(MethodInfo methodInfo)
    {
        string optionalAsync = methodInfo.ReturnType.IsTaskType ? "async " : string.Empty;
        return $"{optionalAsync}{RenderMethodSignatureForInterface(methodInfo)}";
    }

    internal string RenderMethodSignatureForInterface(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}({RenderMethodParameters(methodInfo)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnType.ManagedType, methodInfo.ReturnType.CLRTypeSyntax)}";
    }

    private string RenderMethodParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters
            .Select(p => $"{p.ParameterName}: {typeMapper.ToTypeScriptType(p.Type.ManagedType, p.Type.CLRTypeSyntax)}"));
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