using System.Text;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the signature of the user's C# class annotated with <see cref="TsExportAttribute"/>.
/// The interface hides the interop details from the end user through the proxy class rendered by <see cref="TypescriptUserClassProxyRenderer"/>.
/// </summary>
/// <param name="classInfo"></param>
/// <param name="typeMapper"></param>
internal class TypescriptUserClassInterfaceRenderer(ClassInfo classInfo, TypeScriptTypeMapper typeMapper)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {classInfo.Name} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => !m.IsStatic)) // only instance methods
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
        return string.Join(", ", methodInfo.MethodParameters
            .Where(p => !p.IsInjectedInstanceParameter) // skip the injected instance parameter
            .Select(p => $"{p.ParameterName}: {typeMapper.ToTypeScriptType(p.KnownType, p.CLRTypeSyntax.ToString())}"));
    }
}
