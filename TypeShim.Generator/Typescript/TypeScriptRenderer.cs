using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sourceBuilder = new();

    internal string Render()
    {
        TypescriptConfigRenderer configRenderer = new();
        sourceBuilder.AppendLine(configRenderer.Render());
        RenderInteropInterfaces();        
        RenderUserClasses();
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

    private void RenderUserClasses()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            TypeScriptUserClassNamespaceRenderer namespaceRenderer = new(classInfo, symbolNameProvider);
            sourceBuilder.AppendLine(namespaceRenderer.Render());
        }
    }
}