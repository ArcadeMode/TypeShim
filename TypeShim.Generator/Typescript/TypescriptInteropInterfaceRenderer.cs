using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the generated JSExport-annoted class by <see cref="CSharpInteropClassRenderer"/>
/// </summary>
/// <param name="classInfo"></param>
/// <param name="typeMapper"></param>
internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptClassNameBuilder classNameBuilder)
{
    //TODO: render mirror TypeScript interop class

    // PURPOSE:
    // - provide strongly typed interop interface for a single class instance
    // - recognizable methods matching the original C# class for end user

    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interop interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {classNameBuilder.GetInteropInterfaceName(classInfo)} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            sb.AppendLine($"    {methodRenderer.RenderMethodSignature(methodInfo.WithoutInstanceParameterTypeInfo())};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}
