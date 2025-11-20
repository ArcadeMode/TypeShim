using System.Text;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the generated JSExport-annoted class by <see cref="CSharpInteropClassRenderer"/>
/// </summary>
/// <param name="classInfo"></param>
/// <param name="typeMapper"></param>
internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo, TypeScriptTypeMapper typeMapper)
{
    //TODO: render mirror TypeScript interop class

    // PURPOSE:
    // - provide strongly typed interop interface for a single class instance
    // - recognizable methods matching the original C# class for end user

    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interop interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {TypescriptClassNameBuilder.GetInteropInterfaceName(classInfo)} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            sb.AppendLine($"    {RenderMethodSignature(methodInfo)};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string RenderMethodSignature(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}({RenderMethodParameters(methodInfo)}): {typeMapper.ToTypeScriptType(methodInfo.ReturnKnownType, methodInfo.ReturnCLRTypeSyntax.ToString())}";
    }

    private string RenderMethodParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p => $"{p.ParameterName}: {typeMapper.ToTypeScriptType(p.KnownType, p.CLRTypeSyntax.ToString())}"));
    }
}
