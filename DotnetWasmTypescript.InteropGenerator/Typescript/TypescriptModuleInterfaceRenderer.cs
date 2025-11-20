using System.Text;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the JSExport-generated WebAssembly module exports
/// </summary>
/// <param name="moduleInfo"></param>
internal class TypescriptModuleInterfaceRenderer(WasmModuleInfo moduleInfo)
{   
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        sb.AppendLine("// Auto-generated TypeScript module exports interface");
        sb.AppendLine("export interface WasmModuleExports {"); // FEATURE: allow custom module naming?

        RenderModuleInfo(moduleInfo, 1);

        sb.AppendLine("}");
        return sb.ToString();
    }

    private void RenderModuleInfo(WasmModuleInfo moduleInfo, int indentLevel)
    {
        string indent = new(' ', indentLevel * 4);
        foreach (KeyValuePair<string, WasmModuleInfo> child in moduleInfo.Children)
        {
            if (child.Value.ExportedClass != null)
            {
                sb.AppendLine($"{indent}{child.Key}: {TypescriptClassNameBuilder.GetInteropInterfaceName(child.Value.ExportedClass)};");
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
