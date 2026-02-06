using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(RenderContext ctx)
{
    internal void Render()
    {
        if (ctx.Class.IsStatic) return;

        PropertyInfo[] instancePropertyInfos = [.. ctx.Class.Properties.Where(p => !p.IsStatic && !p.Type.IsDelegateType())];
        PropertyInfo[] initializerPropertyInfos = ctx.Class.Constructor?.MemberInitializers ?? [];
        if (initializerPropertyInfos.Length == 0 && instancePropertyInfos.Length == 0)
            return;

        ctx.AppendLine($"export namespace {ctx.Class.Name} {{");
        using (ctx.Indent())
        {
            TypeScriptUserClassShapesRenderer shapesRenderer = new(ctx);
            if (initializerPropertyInfos.Length > 0)
            {
                shapesRenderer.RenderInitializerInterface(initializerPropertyInfos);
            }

            if (instancePropertyInfos.Length > 0)
            {
                shapesRenderer.RenderPropertiesInterface(instancePropertyInfos);
                const string proxyParamName = "proxy";
                shapesRenderer.RenderPropertiesFunction(proxyParamName);
            }
        }
        ctx.AppendLine("}");
    }
}
