using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Generator.CSharp;
using TypeShim.Generator;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sourceBuilder = new();

    internal string Render()
    {
        TypescriptConfigRenderer configRenderer = new();
        sourceBuilder.AppendLine(configRenderer.Render());
        RenderAssemblyExports();        
        RenderUserClasses();
        return sourceBuilder.ToString();
    }

    private void RenderAssemblyExports()
    {
        RenderContext renderCtx = new(null, classInfos, RenderOptions.TypeScript);
        TypescriptAssemblyExportsRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, symbolNameProvider, renderCtx);
        sourceBuilder.AppendLine(moduleInterfaceRenderer.Render());
    }

    private void RenderUserClasses()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            RenderContext renderCtx = new(classInfo, classInfos, RenderOptions.TypeScript);

            TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(classInfo, symbolNameProvider, renderCtx);
            sourceBuilder.AppendLine(namespaceRenderer.Render());
        }
    }
}