using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(RenderContext ctx)
{
    internal void Render()
    {
        PropertyInfo[] instancePropertyInfos = [.. ctx.Class.Properties.Where(p => !p.IsStatic && !p.Type.IsDelegateType())];
        PropertyInfo[] initializerPropertyInfos = ctx.Class.Constructor?.MemberInitializers ?? [];
        IReadOnlyList<NamedTypeInfo> nestedTypes = ctx.Class.NestedTypes;

        bool hasInitializerOrSnapshot = initializerPropertyInfos.Length > 0 || instancePropertyInfos.Length > 0;
        if (!hasInitializerOrSnapshot && nestedTypes.Count == 0)
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

    private void RenderNestedTypes(IReadOnlyList<NamedTypeInfo> nestedTypes)
    {
        foreach (NamedTypeInfo nestedType in nestedTypes)
        {
            RenderContext nestedCtx = ctx.GetNestedContext(nestedType);
            switch (nestedType)
            {
                case ClassInfo:
                    new TypescriptUserClassProxyRenderer(nestedCtx).Render();
                    new TypeScriptUserClassNamespaceRenderer(nestedCtx).Render();
                    break;
                case EnumInfo:
                    new TypeScriptEnumRenderer(nestedCtx).Render();
                    break;
            }
        }
    }
}
