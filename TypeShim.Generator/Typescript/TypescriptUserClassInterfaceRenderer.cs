using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the signature of the user's C# class annotated with <see cref="TsExportAttribute"/>.
/// The interface hides the interop details from the end user through the proxy class rendered by <see cref="TypescriptUserClassProxyRenderer"/>.
/// </summary>
/// <param name="classInfo"></param>
/// <param name="typeMapper"></param>
internal class TypescriptUserClassInterfaceRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypeScriptTypeMapper typeMapper)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {classInfo.Name} {{");
        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => !m.IsStatic)) // only instance methods
        {
            sb.AppendLine($"    {methodRenderer.RenderMethodSignatureForInterface(methodInfo.WithoutInstanceParameter())};");
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => !p.IsStatic))
        {
            bool isReadonly = propertyInfo.SetMethod is null;
            sb.AppendLine($"    {(isReadonly ? "readonly " : string.Empty)}{propertyInfo.Name}: {typeMapper.ToTypeScriptType(propertyInfo.Type)};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}
