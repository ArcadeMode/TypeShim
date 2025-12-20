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
        foreach ((MethodInfo methodInfo, MethodOverloadInfo? overloadInfo) in GetAllMethods())
        {
            sb.AppendLine($"    {RenderInteropMethodSignature(methodInfo, overloadInfo)};");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string RenderInteropMethodSignature(MethodInfo methodInfo, MethodOverloadInfo? overloadInfo = null)
    {
        string returnType = symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
        return $"{overloadInfo?.Name ?? methodInfo.Name}({RenderInteropMethodParameters(overloadInfo?.MethodParameters ?? methodInfo.MethodParameters)}): {returnType}";

        string RenderInteropMethodParameters(IEnumerable<MethodParameterInfo> parameterInfos)
        {
            return string.Join(", ", parameterInfos.Select(p => $"{p.Name}: {symbolNameProvider.GetNakedSymbolReference(p.Type)}"));
        }
    }

    private IEnumerable<(MethodInfo MethodInfo, MethodOverloadInfo? OverloadInfo)> GetAllMethods()
    {
        foreach (MethodInfo methodInfo in classInfo.Methods.Select(m => m.WithInteropTypeInfo()))
        {
            yield return (methodInfo, null);
            foreach (MethodOverloadInfo overloadInfo in methodInfo.Overloads)
            {
                yield return (methodInfo, overloadInfo);
            }
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Select(p => p.WithInteropTypeInfo()))
        {
            yield return (propertyInfo.GetMethod, null);
            foreach (MethodOverloadInfo overloadInfo in propertyInfo.GetMethod.Overloads)
            {
                yield return (propertyInfo.GetMethod, overloadInfo);
            }
            if (propertyInfo.SetMethod is not MethodInfo setMethod)
            {
                continue;
            }
            yield return (setMethod, null);
            foreach (MethodOverloadInfo overloadInfo in setMethod.Overloads)
            {
                yield return (setMethod, overloadInfo);
            }
        }
    }
}
