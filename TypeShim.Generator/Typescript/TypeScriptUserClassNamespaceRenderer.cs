using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    internal void Render()
    {
        ctx.AppendLine($"// Auto-generated TypeScript namespace for class: {ctx.Class.Namespace}.{ctx.Class.Name}")
           .AppendLine($"export namespace {ctx.Class.Name} {{");
        using (ctx.Indent())
        {
            TypescriptUserClassProxyRenderer proxyRenderer = new(symbolNameProvider, ctx);
            proxyRenderer.Render();

            TypeScriptUserClassShapesRenderer propertiesRenderer = new(symbolNameProvider, ctx);
            propertiesRenderer.Render();
        }
        ctx.AppendLine("}");
    }
}
