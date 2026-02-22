using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator;

internal class TypeScriptRenderer(List<ClassInfo> classInfos, ModuleInfo moduleInfo)
{
    internal void Render(StreamWriter tsWriter)
    {
        RenderContext configCtx = RenderTypeShimConfig();
        tsWriter.WriteLine(configCtx.ToString());
        RenderContext exportsCtx = RenderAssemblyExports();
        tsWriter.WriteLine(exportsCtx.ToString());

        RenderContext[] classCtxs = new RenderContext[classInfos.Count];
        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = Math.Min(4, Environment.ProcessorCount)
        };
        Parallel.For(0, classInfos.Count, options, i =>
        {
            classCtxs[i] = RenderUserClass(classInfos[i]);
        });
        foreach (RenderContext ctx in classCtxs)
        {
            tsWriter.WriteLine(ctx.ToString()); 
        }
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