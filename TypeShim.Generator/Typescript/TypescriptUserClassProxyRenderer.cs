using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();
    private readonly TypeScriptMethodRenderer methodRenderer = new(classInfo, symbolNameProvider);

    internal string Render(int depth)
    {
        string interopInterfaceName = symbolNameProvider.GetModuleInteropClassName();
        RenderProxyClass(interopInterfaceName, depth);
        return sb.ToString();
    }

    private void RenderProxyClass(string interopInterfaceName, int depth)
    {
        string indent = new(' ', depth * 2);
        string indent2 = new(' ', (depth + 1) * 2);
        string indent3 = new(' ', (depth + 2) * 2);

        sb.AppendLine($"{indent}export class {symbolNameProvider.GetProxyDefinitionName()} {{");
        sb.AppendLine($"{indent2}interop: {interopInterfaceName};");
        sb.AppendLine($"{indent2}instance: object;");
        sb.AppendLine();
        sb.AppendLine($"{indent2}constructor(instance: object, interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent3}this.interop = interop;");
        sb.AppendLine($"{indent3}this.instance = instance;");
        sb.AppendLine($"{indent2}}}");
        sb.AppendLine();

        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => !m.IsStatic))
        {
            methodRenderer.RenderProxyMethod(depth + 1, methodInfo);
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => !p.IsStatic && p.Type.IsSnapshotCompatible))
        {
            methodRenderer.RenderProxyProperty(depth + 1, propertyInfo);
        }
        sb.Append(methodRenderer.GetRenderedContent());
        sb.AppendLine($"{indent}}}");
    }
}
