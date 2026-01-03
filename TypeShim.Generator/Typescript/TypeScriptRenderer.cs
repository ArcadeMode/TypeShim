using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator.CSharp;
using TypeShim.Generator;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo)
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

    private IEnumerable<RenderContext> RenderUserClasses()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            RenderContext renderCtx = new(classInfo, classInfos, RenderOptions.TypeScript);
            renderCtx.AppendLine($"// TypeShim generated TypeScript definitions for class: {renderCtx.Class.Namespace}.{renderCtx.Class.Name}");
            TypescriptUserClassProxyRenderer proxyRenderer = new(renderCtx);
            proxyRenderer.Render();
            TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(renderCtx);
            namespaceRenderer.Render();
            yield return renderCtx;
        }
    }
}