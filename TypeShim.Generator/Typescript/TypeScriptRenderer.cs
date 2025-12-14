using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sourceBuilder = new();
    private readonly TypeScriptMethodRenderer methodRenderer = new(symbolNameProvider);

    internal string Render()
    {
        RenderInteropModuleInterface();
        RenderUserModuleClass();

        RenderInteropInterfaces();
        RenderUserClassNamespaces();
        return sourceBuilder.ToString();
    }

    private void RenderInteropModuleInterface()
    {
        TypescriptInteropModuleInterfaceRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, symbolNameProvider);
        sourceBuilder.AppendLine(moduleInterfaceRenderer.Render());
    }

    private void RenderInteropInterfaces()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptInteropInterfaceRenderer interopInterfaceRenderer = new(classInfo, methodRenderer, symbolNameProvider);
            sourceBuilder.AppendLine(interopInterfaceRenderer.Render());
        }
    }

    private void RenderUserClassNamespaces()
    {
        foreach (ClassInfo classInfo in classInfos.Where(c => !c.IsModule))
        {
            TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(classInfo, methodRenderer, symbolNameProvider);
            sourceBuilder.AppendLine(namespaceRenderer.Render());
        }
    }

    private void RenderUserModuleClass()
    {
        foreach (ClassInfo moduleClassInfo in classInfos.Where(c => c.IsModule))
        {
            TypescriptUserModuleClassRenderer moduleClassRenderer = new(moduleClassInfo, methodRenderer, symbolNameProvider);
            sourceBuilder.AppendLine(moduleClassRenderer.Render());
        }
    }
}