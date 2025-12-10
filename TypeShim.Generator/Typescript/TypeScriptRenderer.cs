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
        RenderUserModuleClass();

        RenderInteropInterfaces();
        RenderUserClassInterfaces();

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
        foreach (ClassInfo classInfo in classInfos.Where(c => !c.IsModule))
        {
            TypescriptUserClassInterfaceRenderer classInterfaceRenderer = new(classInfo, methodRenderer, typeMapper);
            sourceBuilder.AppendLine(classInterfaceRenderer.Render());
        }
    }

    private void RenderUserModuleClass()
    {
        foreach (ClassInfo moduleClassInfo in classInfos.Where(c => c.IsModule))
        {
            TypescriptUserModuleClassRenderer moduleClassRenderer = new(moduleClassInfo, methodRenderer, classNameBuilder);
            sourceBuilder.AppendLine(moduleClassRenderer.Render());
        }
    }

    private void RenderUserClassProxies()
    {
        foreach (ClassInfo classInfo in classInfos.Where(c => !c.IsModule))
        {
            TypescriptUserClassProxyRenderer classProxyRenderer = new(classInfo, methodRenderer, classNameBuilder);
            sourceBuilder.AppendLine(classProxyRenderer.Render());
        }
    }
}