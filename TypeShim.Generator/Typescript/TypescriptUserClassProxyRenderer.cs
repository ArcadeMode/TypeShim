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

        sb.Append($"{indent}export class {symbolNameProvider.GetUserClassProxySymbolName()}");
        if (!classInfo.IsStatic)
        {
            sb.Append($" extends ProxyBase");
        }
        sb.AppendLine(" {");

        RenderConstructor(depth + 1);

        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            methodRenderer.RenderProxyMethod(depth + 1, methodInfo);
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => p.IsSnapshotCompatible() || p.IsStatic))
        {
            methodRenderer.RenderProxyProperty(depth + 1, propertyInfo);
        }
        sb.Append(methodRenderer.GetRenderedContent());
        sb.AppendLine($"{indent}}}");
    }

    private void RenderConstructor(int depth)
    {
        string indent = new(' ', depth * 2);
        string indent2 = new(' ', (depth + 1) * 2);
        
        if (classInfo.IsStatic)
        {
            sb.AppendLine($"{indent}private constructor() {{}}");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine($"{indent}constructor() {{");
            sb.AppendLine($"{indent2}super(null!);"); //TODO: render constructor interop call
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }
    }
}
