using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    private readonly StringBuilder sb = new();
    private readonly TypeScriptMethodRenderer methodRenderer = new(classInfo, symbolNameProvider, ctx);

    internal string Render()
    {
        string interopInterfaceName = symbolNameProvider.GetModuleInteropClassName();
        RenderProxyClass(interopInterfaceName);
        return ctx.Render();
    }

    private void RenderProxyClass(string interopInterfaceName)
    {
        ctx.Append($"export class ").Append(symbolNameProvider.GetUserClassProxySymbolName());
        if (!classInfo.IsStatic)
        {
            ctx.Append($" extends ProxyBase");
        }
        ctx.AppendLine(" {");

        using (ctx.Indent())
        {
            RenderConstructor();
            foreach (MethodInfo methodInfo in classInfo.Methods)
            {
                methodRenderer.RenderProxyMethod(methodInfo);
            }
            foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => p.IsSnapshotCompatible() || p.IsStatic))
            {
                methodRenderer.RenderProxyProperty(propertyInfo);
            }
        }
        ctx.AppendLine("}");
    }

    private void RenderConstructor()
    {        
        if (classInfo.IsStatic)
        {
            ctx.AppendLine("private constructor() {}")
                     .AppendLine();
        }
        else
        {
            ctx.AppendLine($"constructor() {{");
            using (ctx.Indent())
            {
                ctx.AppendLine("super(null!);"); //TODO: render constructor interop call
            }
            ctx.AppendLine("}")
                     .AppendLine();
        }
    }
}
