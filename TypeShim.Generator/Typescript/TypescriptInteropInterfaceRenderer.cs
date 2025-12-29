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
/// <param name="symbolNameProvider"></param>
internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript interop interface. Source class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export interface {symbolNameProvider.GetInteropInterfaceName(classInfo)} {{");
        // TODO: add depth param.
        // TODO: consider merging with module rendering (mode param?)
        foreach (MethodInfo methodInfo in GetAllMethods())
        {
            sb.AppendLine($"    {RenderInteropMethodSignature(methodInfo)};");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string RenderInteropMethodSignature(MethodInfo methodInfo)
    {
        string returnType = symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
        return $"{methodInfo.Name}({RenderInteropMethodParameters(methodInfo.Parameters)}): {returnType}";

        string RenderInteropMethodParameters(IEnumerable<MethodParameterInfo> parameterInfos)
        {
            return string.Join(", ", parameterInfos.Select(p => $"{p.Name}: {symbolNameProvider.GetNakedSymbolReference(p.Type)}"));
        }
    }

    private IEnumerable<MethodInfo> GetAllMethods()
    {
        foreach (MethodInfo methodInfo in classInfo.Methods.Select(m => m.WithInteropTypeInfo()))
        {
            yield return methodInfo;
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Select(p => p.WithInteropTypeInfo()))
        {
            yield return propertyInfo.GetMethod;

            if (propertyInfo.SetMethod is not MethodInfo setMethod)
            {
                continue;
            }
            yield return setMethod;
            // Note: init is not rendered as an interop method.
        }
    }
}
