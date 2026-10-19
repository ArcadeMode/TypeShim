using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator;

internal class TypeScriptRenderer(RenderContext ctx, List<NamedTypeInfo> namedTypeInfos, ModuleInfo moduleInfo)
{
    internal void Render()
    {
        RenderTypeShimConfig();
        RenderAssemblyExports();
        foreach (NamedTypeInfo namedType in namedTypeInfos)
        {
            switch (namedType)
            {
                case ClassInfo classInfo:
                    RenderUserClass(classInfo);
                    break;
                case EnumInfo enumInfo:
                    RenderUserEnum(enumInfo);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported named type: {namedType.GetType().Name}");
            }
        }
    }

    private void RenderTypeShimConfig()
    {
        RenderContext configCtx = ctx.GetNestedContext();
        TypeScriptPreambleRenderer configRenderer = new(configCtx);
        configRenderer.Render();
        ctx.AppendLine();
    }

    private void RenderAssemblyExports()
    {
        RenderContext renderCtx = ctx.GetNestedContext();
        TypescriptAssemblyExportsRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, renderCtx);
        moduleInterfaceRenderer.Render();
        ctx.AppendLine();
    }

    private void RenderUserClass(ClassInfo classInfo)
    {
        RenderContext renderCtx = ctx.GetNestedContext(classInfo);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for class: {renderCtx.Class.Namespace}.{renderCtx.Class.Name}");
        TypescriptUserClassProxyRenderer proxyRenderer = new(renderCtx);
        proxyRenderer.Render();
        TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(renderCtx);
        namespaceRenderer.Render();
        ctx.AppendLine();
    }

    private void RenderUserEnum(EnumInfo enumInfo)
    {
        RenderContext renderCtx = ctx.GetNestedContext(enumInfo);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for enum: {enumInfo.Namespace}.{enumInfo.Name}");
        TypeScriptEnumRenderer enumRenderer = new(renderCtx);
        enumRenderer.Render();
        ctx.AppendLine();
    }
}
