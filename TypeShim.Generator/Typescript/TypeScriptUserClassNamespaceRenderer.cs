using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    internal void Render()
    {
        ctx.AppendLine($"// Auto-generated TypeScript namespace for class: {classInfo.Namespace}.{classInfo.Name}")
           .AppendLine($"export namespace {symbolNameProvider.GetUserClassNamespace(classInfo)} {{");
        using (ctx.Indent())
        {
            TypescriptUserClassProxyRenderer proxyRenderer = new(classInfo, symbolNameProvider, ctx);
            proxyRenderer.Render();
            if (classInfo.IsSnapshotCompatible())
            {
                TypeScriptUserClassSnapshotRenderer snapshotRenderer = new(classInfo, symbolNameProvider, ctx);
                snapshotRenderer.Render();
            }
        }
        ctx.AppendLine("}");
    }
}
