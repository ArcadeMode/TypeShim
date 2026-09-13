using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(RenderContext ctx)
{
    internal void Render()
    {
        PropertyInfo[] instancePropertyInfos = ctx.Class.IsStatic
            ? []
            : [.. ctx.Class.Properties.Where(p => !p.IsStatic && !p.Type.IsDelegateType())];
        PropertyInfo[] initializerPropertyInfos = ctx.Class.IsStatic ? [] : ctx.Class.Constructor?.MemberInitializers ?? [];
        IReadOnlyList<NamedTypeInfo> nestedTypes = ctx.Class.NestedTypes;

        bool hasShapes = initializerPropertyInfos.Length > 0 || instancePropertyInfos.Length > 0;
        if (!hasShapes && nestedTypes.Count == 0)
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

            RenderNestedTypes(nestedTypes);
        }
        ctx.AppendLine("}");
    }

    // Recurses each nested type into its own RenderContext and injects the (re-indented) output inside the
    // parent's namespace, yielding e.g. `new Shipment.Manifest()` and `Shipment.Status.Completed`.
    private void RenderNestedTypes(IReadOnlyList<NamedTypeInfo> nestedTypes)
    {
        foreach (NamedTypeInfo nestedType in nestedTypes)
        {
            RenderContext childCtx = new(nestedType, ctx.AllNamedTypes, RenderOptions.TypeScript);
            switch (nestedType)
            {
                case ClassInfo:
                    new TypescriptUserClassProxyRenderer(childCtx).Render();
                    new TypeScriptUserClassNamespaceRenderer(childCtx).Render();
                    break;
                case EnumInfo:
                    new TypeScriptEnumRenderer(childCtx).Render();
                    break;
            }
            ctx.AppendIndentedBlock(childCtx.ToString());
        }
    }
}
