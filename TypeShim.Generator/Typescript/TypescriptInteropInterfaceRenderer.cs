using System.Reflection;
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
/// <param name="classNameBuilder"></param>
internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptClassNameBuilder classNameBuilder)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interop interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {classNameBuilder.GetInteropInterfaceName(classInfo)} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            sb.AppendLine($"    {methodRenderer.RenderMethodSignatureForInterface(methodInfo.WithoutInstanceParameterTypeInfo())};");
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties)
        {
            sb.AppendLine($"    {methodRenderer.RenderMethodSignatureForInterface(propertyInfo.GetMethod.WithoutInstanceParameterTypeInfo())};");
            if (propertyInfo.SetMethod is MethodInfo setMethod)
            {
                sb.AppendLine($"    {methodRenderer.RenderMethodSignatureForInterface(setMethod.WithoutInstanceParameterTypeInfo())};");
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}
