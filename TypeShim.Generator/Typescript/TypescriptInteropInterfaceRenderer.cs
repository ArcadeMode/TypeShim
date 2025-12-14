using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the generated JSExport-annoted class by <see cref="CSharpInteropClassRenderer"/>.<br/>
/// The interface provides strongly typed interop methods for a single class instance with method signatures matching the original C# class annotated by end user.
/// </summary>
/// <param name="classInfo"></param>
/// <param name="methodRenderer"></param>
/// <param name="symbolNameProvider"></param>
internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interop interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {symbolNameProvider.GetInteropInterfaceName(classInfo)} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            sb.AppendLine($"    {methodRenderer.RenderInteropMethodSignature(methodInfo.WithInteropTypeInfo())};");
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties)
        {
            sb.AppendLine($"    {methodRenderer.RenderInteropMethodSignature(propertyInfo.GetMethod.WithInteropTypeInfo())};");
            if (propertyInfo.SetMethod is MethodInfo setMethod)
            {
                sb.AppendLine($"    {methodRenderer.RenderInteropMethodSignature(setMethod.WithInteropTypeInfo())};");
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}
