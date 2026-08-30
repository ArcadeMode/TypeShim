using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator;

internal class TypeScriptRenderer(List<NamedTypeInfo> namedTypeInfos, ModuleInfo moduleInfo)
{
    internal List<RenderContext> Render()
    {
        List<RenderContext> renderContexts = new(namedTypeInfos.Count + 2)
        {
            RenderTypeShimConfig(),
            RenderAssemblyExports()
        };
        foreach (NamedTypeInfo namedType in namedTypeInfos)
        {
            renderContexts.Add(namedType switch
            {
                ClassInfo classInfo => RenderUserClass(classInfo),
                EnumInfo enumInfo => RenderUserEnum(enumInfo),
                _ => throw new InvalidOperationException($"Unsupported named type: {namedType.GetType().Name}"),
            });
        }
        return renderContexts;
    }

    private RenderContext RenderTypeShimConfig()
    {
        RenderContext configCtx = new(null, namedTypeInfos, RenderOptions.TypeScript);
        TypeScriptPreambleRenderer configRenderer = new(configCtx);
        configRenderer.Render();
        return configCtx;
    }

    private RenderContext RenderAssemblyExports()
    {
        RenderContext renderCtx = new(null, namedTypeInfos, RenderOptions.TypeScript);
        TypescriptAssemblyExportsRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, renderCtx);
        moduleInterfaceRenderer.Render();
        return renderCtx;
    }
    
    private RenderContext RenderUserClass(ClassInfo classInfo)
    {
        RenderContext renderCtx = new(classInfo, namedTypeInfos, RenderOptions.TypeScript);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for class: {renderCtx.Class.Namespace}.{renderCtx.Class.Name}");
        TypescriptUserClassProxyRenderer proxyRenderer = new(renderCtx);
        proxyRenderer.Render();
        TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(renderCtx);
        namespaceRenderer.Render();
        return renderCtx;
    }

    private RenderContext RenderUserEnum(EnumInfo enumInfo)
    {
        RenderContext renderCtx = new(enumInfo, namedTypeInfos, RenderOptions.TypeScript);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for enum: {enumInfo.Namespace}.{enumInfo.Name}");
        TypeScriptEnumRenderer enumRenderer = new(renderCtx);
        enumRenderer.Render();
        return renderCtx;
    }
}