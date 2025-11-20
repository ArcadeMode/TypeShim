using System.Text;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the JSExport-generated WebAssembly module exports
/// </summary>
/// <param name="moduleHierarchyInfo"></param>
internal class TypescriptInteropModuleInterfaceRenderer(ModuleHierarchyInfo moduleHierarchyInfo, TypescriptClassNameBuilder classNameBuilder)
{   
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine("// Auto-generated TypeScript module exports interface");
        sb.AppendLine($"export interface {classNameBuilder.GetModuleInteropClassName()} {{");

        RenderModuleInfo(moduleHierarchyInfo, 1);

        sb.AppendLine("}");
        return sb.ToString();
    }

    private void RenderModuleInfo(ModuleHierarchyInfo moduleInfo, int indentLevel)
    {
        string indent = new(' ', indentLevel * 4);
        foreach (KeyValuePair<string, ModuleHierarchyInfo> child in moduleInfo.Children)
        {
            if (child.Value.ExportedClass != null)
            {
                sb.AppendLine($"{indent}{child.Key}: {classNameBuilder.GetInteropInterfaceName(child.Value.ExportedClass)};");
            }
            else
            {
                sb.AppendLine($"{indent}{child.Key}: {{");
                RenderModuleInfo(child.Value, indentLevel + 1);
                sb.AppendLine($"{indent}}};");
            }
        }
    }
}
