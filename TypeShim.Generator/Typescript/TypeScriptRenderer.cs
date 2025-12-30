using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator.CSharp;
using TypeShim.Generator;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        foreach(RenderContext ctx in (RenderContext[])[RenderTypeShimConfig(), RenderAssemblyExports(), .. RenderUserClasses()])
        {
            sb.AppendLine(ctx.ToString());
        }
        return sb.ToString();
    }

    private RenderContext RenderTypeShimConfig()
    {
        RenderContext configCtx = new(null, classInfos, RenderOptions.TypeScript);
        TypescriptConfigRenderer configRenderer = new(configCtx);
        configRenderer.Render();
        return configCtx;
    }

    private RenderContext RenderAssemblyExports()
    {
        RenderContext renderCtx = new(null, classInfos, RenderOptions.TypeScript);
        TypescriptAssemblyExportsRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, symbolNameProvider, renderCtx);
        moduleInterfaceRenderer.Render();
        return renderCtx;
    }

    private IEnumerable<RenderContext> RenderUserClasses()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            RenderContext renderCtx = new(classInfo, classInfos, RenderOptions.TypeScript);
            TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(symbolNameProvider, renderCtx);
            namespaceRenderer.Render();
            yield return renderCtx;
        }
    }
}