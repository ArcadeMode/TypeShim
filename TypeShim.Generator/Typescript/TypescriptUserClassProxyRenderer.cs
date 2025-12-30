using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptUserClassProxyRenderer(TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    private readonly TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider, ctx);

    internal void Render()
    {
        ctx.Append($"export class ").Append(RenderConstants.Proxy);
        if (!ctx.Class.IsStatic)
        {
            ctx.Append($" extends ProxyBase");
        }
        ctx.AppendLine(" {");

        using (ctx.Indent())
        {
            methodRenderer.RenderProxyConstructor(ctx.Class.Constructor);
            foreach (MethodInfo methodInfo in ctx.Class.Methods)
            {
                methodRenderer.RenderProxyMethod(methodInfo);
            }
            foreach (PropertyInfo propertyInfo in ctx.Class.Properties.Where(p => p.IsSnapshotCompatible() || p.IsStatic))
            {
                methodRenderer.RenderProxyProperty(propertyInfo);
            }
        }
        ctx.AppendLine("}");
    }
}
