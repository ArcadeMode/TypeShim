using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator;

internal class TypeScriptRenderer(List<NamedTypeInfo> namedTypeInfos, ModuleInfo moduleInfo)
{
    internal RenderContext Render()
    {
        RenderContext rootCtx = new(null, namedTypeInfos, RenderOptions.TypeScript);

        RenderTypeShimConfig(rootCtx);
        RenderAssemblyExports(rootCtx);
        foreach (NamedTypeInfo namedType in namedTypeInfos)
        {
            switch (namedType)
            {
                case ClassInfo classInfo:
                    RenderUserClass(rootCtx, classInfo);
                    break;
                case EnumInfo enumInfo:
                    RenderUserEnum(rootCtx, enumInfo);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported named type: {namedType.GetType().Name}");
            }
        }
        return rootCtx;
    }

    private void RenderTypeShimConfig(RenderContext rootCtx)
    {
        RenderContext configCtx = rootCtx.GetNestedContext();
        TypeScriptPreambleRenderer configRenderer = new(configCtx);
        configRenderer.Render();
        rootCtx.AppendLine();
    }

    private void RenderAssemblyExports(RenderContext rootCtx)
    {
        RenderContext renderCtx = rootCtx.GetNestedContext();
        TypescriptAssemblyExportsRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, renderCtx);
        moduleInterfaceRenderer.Render();
        rootCtx.AppendLine();
    }

    private void RenderUserClass(RenderContext rootCtx, ClassInfo classInfo)
    {
        RenderContext renderCtx = rootCtx.GetNestedContext(classInfo);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for class: {renderCtx.Class.Namespace}.{renderCtx.Class.Name}");
        TypescriptUserClassProxyRenderer proxyRenderer = new(renderCtx);
        proxyRenderer.Render();
        TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(renderCtx);
        namespaceRenderer.Render();
        rootCtx.AppendLine();
    }

    private void RenderUserEnum(RenderContext rootCtx, EnumInfo enumInfo)
    {
        RenderContext renderCtx = rootCtx.GetNestedContext(enumInfo);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for enum: {enumInfo.Namespace}.{enumInfo.Name}");
        TypeScriptEnumRenderer enumRenderer = new(renderCtx);
        enumRenderer.Render();
        rootCtx.AppendLine();
    }
}
