using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(RenderContext ctx)
{
    internal void Render()
    {
        if (ctx.Class.IsStatic) return;

        PropertyInfo[] instancePropertyInfos = [.. ctx.Class.Properties.Where(p => !p.IsStatic)];
        if (instancePropertyInfos.Length == 0)
            return;

        ctx.AppendLine($"export namespace {ctx.Class.Name} {{");
        using (ctx.Indent())
        {
            TypeScriptUserClassShapesRenderer shapesRenderer = new(ctx);
            if (ctx.Class.Constructor?.MemberInitializers is { Length: > 0 } initializerPropertyInfos)
            {
                shapesRenderer.RenderInitializerInterface(initializerPropertyInfos);
            }
            shapesRenderer.RenderPropertiesInterface(instancePropertyInfos);
            const string proxyParamName = "proxy";
            shapesRenderer.RenderPropertiesFunction(proxyParamName);
        }
        ctx.AppendLine("}");
    }
}
