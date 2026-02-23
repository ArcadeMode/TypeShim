using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator;

internal class TypeScriptRenderer(List<ClassInfo> classInfos, ModuleInfo moduleInfo)
{
    internal List<RenderContext> Render()
    {
        List<RenderContext> renderContexts = new(classInfos.Count + 2)
        {
            RenderTypeShimConfig(),
            RenderAssemblyExports()
        };
        foreach (ClassInfo classInfo in classInfos)
        {
            renderContexts.Add(RenderUserClass(classInfo));
        }
        return renderContexts;
    }

    private RenderContext RenderTypeShimConfig()
    {
        RenderContext configCtx = new(null, classInfos, RenderOptions.TypeScript);
        TypeScriptPreambleRenderer configRenderer = new(configCtx);
        configRenderer.Render();
        return configCtx;
    }

    private RenderContext RenderAssemblyExports()
    {
        RenderContext renderCtx = new(null, classInfos, RenderOptions.TypeScript);
        TypescriptAssemblyExportsRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, renderCtx);
        moduleInterfaceRenderer.Render();
        return renderCtx;
    }
    
    private RenderContext RenderUserClass(ClassInfo classInfo)
    {
        RenderContext renderCtx = new(classInfo, classInfos, RenderOptions.TypeScript);
        renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for class: {renderCtx.Class.Namespace}.{renderCtx.Class.Name}");
        TypescriptUserClassProxyRenderer proxyRenderer = new(renderCtx);
        proxyRenderer.Render();
        TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(renderCtx);
        namespaceRenderer.Render();
        return renderCtx;
    }
}