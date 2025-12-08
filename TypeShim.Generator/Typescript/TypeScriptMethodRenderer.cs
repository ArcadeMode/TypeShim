using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptMethodRenderer(TypeScriptTypeMapper typeMapper)
{
    internal string RenderPropertyGetterSignatureForClass(MethodInfo methodInfo)
    {

        return $"get {methodInfo.Name.TrimStart("get_")}({RenderMethodParameters(methodInfo)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnType)}";
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
    {//BEEPBOOP
        return $"{methodInfo.Name}({RenderMethodParameters(methodInfo)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnType)}";
    }

    private string RenderMethodParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters
            .Select(p => $"{p.Name}: {typeMapper.ToTypeScriptType(p.Type)}"));
    }
}