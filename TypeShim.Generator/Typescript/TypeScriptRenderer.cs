using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sourceBuilder = new();

    internal string Render()
    {
        RenderInteropInterfaces();
        
        RenderUserModuleClass();
        RenderUserClassNamespaces();
        return sourceBuilder.ToString();
    }

    private void RenderInteropInterfaces()
    {
        TypescriptInteropModuleInterfaceRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, symbolNameProvider);
        sourceBuilder.AppendLine(moduleInterfaceRenderer.Render());
        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptInteropInterfaceRenderer interopInterfaceRenderer = new(classInfo, symbolNameProvider);
            sourceBuilder.AppendLine(interopInterfaceRenderer.Render());
        }
    }

    private void RenderUserClassNamespaces()
    {
        foreach (ClassInfo classInfo in classInfos.Where(c => !c.IsModule))
        {
            TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(classInfo, symbolNameProvider);
            sourceBuilder.AppendLine(namespaceRenderer.Render());
        }
    }

    private void RenderUserModuleClass()
    {
        foreach (ClassInfo moduleClassInfo in classInfos.Where(c => c.IsModule))
        {
            TypescriptUserModuleClassRenderer moduleClassRenderer = new(moduleClassInfo, symbolNameProvider);
            sourceBuilder.AppendLine(moduleClassRenderer.Render());
        }
    }
}