using DotnetWasmTypescript.InteropGenerator;
using DotnetWasmTypescript.InteropGenerator.Typescript;
using System.Text;

internal class TypeScriptRenderer(IEnumerable<ClassInfo> classInfos)
{
    private readonly StringBuilder sourceBuilder = new();
    private readonly TypeScriptTypeMapper typeMapper = new(classInfos);
    private readonly WasmModuleInfo moduleInfo = WasmModuleInfo.FromClasses(classInfos);

    internal string Render()
    {
        RenderInteropInterfaces();
        return sourceBuilder.ToString();
    }

    private void RenderInteropInterfaces()
    {
        TypescriptModuleInterfaceRenderer moduleInterfaceRenderer = new(moduleInfo);
        sourceBuilder.AppendLine(moduleInterfaceRenderer.Render());

        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptInteropInterfaceRenderer interopInterfaceRenderer = new(classInfo, typeMapper);
            sourceBuilder.AppendLine(interopInterfaceRenderer.Render());
        }

        foreach (ClassInfo classInfo in classInfos)
        {
            TypescriptUserClassInterfaceRenderer interopInterfaceRenderer = new(classInfo, typeMapper);
            sourceBuilder.AppendLine(interopInterfaceRenderer.Render());
        }
        
    }
}