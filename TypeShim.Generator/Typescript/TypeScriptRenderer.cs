using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos, ModuleInfo moduleInfo, TypescriptClassNameBuilder classNameBuilder, TypeScriptTypeMapper typeMapper)
{
    private readonly StringBuilder sourceBuilder = new();
    private readonly TypeScriptMethodRenderer methodRenderer = new(typeMapper);

    internal string Render()
    {
        RenderInteropModuleInterface();
        RenderInteropInterfaces();
        RenderUserClassInterfaces();

        RenderUserModuleClass();
        RenderUserClassProxies();
        return sourceBuilder.ToString();
    }

    private void RenderInteropModuleInterface()
    {
        TypescriptInteropModuleInterfaceRenderer moduleInterfaceRenderer = new(moduleInfo.HierarchyInfo, classNameBuilder);
        sourceBuilder.AppendLine(moduleInterfaceRenderer.Render());
    }

    private void RenderInteropInterfaces()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptInteropInterfaceRenderer interopInterfaceRenderer = new(classInfo, methodRenderer, classNameBuilder);
            sourceBuilder.AppendLine(interopInterfaceRenderer.Render());
        }
    }

    private void RenderUserClassInterfaces()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptUserClassInterfaceRenderer classInterfaceRenderer = new(classInfo, methodRenderer);
            sourceBuilder.AppendLine(classInterfaceRenderer.Render());
        }
    }

    private void RenderUserModuleClass()
    {
        TypescriptUserModuleClassRenderer moduleClassRenderer = new(moduleInfo, classNameBuilder);
        sourceBuilder.AppendLine(moduleClassRenderer.Render());
    }

    private void RenderUserClassProxies()
    {
        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptUserClassProxyRenderer classProxyRenderer = new(classInfo, methodRenderer, classNameBuilder);
            sourceBuilder.AppendLine(classProxyRenderer.Render());
        }
    }
}