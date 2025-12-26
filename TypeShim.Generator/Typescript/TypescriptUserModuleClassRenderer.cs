using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders the 'TSModule' marked by the user. Can be constructed with the 'AssemblyExports', from here on out the user can interact with their C# code
/// </summary>
internal class TypescriptUserModuleClassRenderer(ClassInfo moduleClassInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();
    private readonly TypeScriptMethodRenderer methodRenderer = new(moduleClassInfo, symbolNameProvider);
    internal string Render()
    {
        RenderModuleClass(moduleClassInfo.Name, symbolNameProvider.GetModuleInteropClassName());
        return sb.ToString();
    }

    private void RenderModuleClass(string className, string interopInterfaceName)
    {
        sb.AppendLine($"// Auto-generated TypeShim TSModule class. Source class: {moduleClassInfo.Namespace}.{moduleClassInfo.Name}");

        sb.AppendLine($"export class {className} {{");
        foreach (MethodInfo methodInfo in moduleClassInfo.Methods.Where(m => m.IsStatic))
        {
            methodRenderer.RenderProxyMethod(depth: 1, methodInfo);
        }

        foreach (PropertyInfo propertyInfo in moduleClassInfo.Properties.Where(p => p.IsStatic))
        {
            methodRenderer.RenderProxyProperty(depth: 1, propertyInfo);
        }
        sb.Append(methodRenderer.GetRenderedContent());
        sb.AppendLine($"}}");
    }
}
