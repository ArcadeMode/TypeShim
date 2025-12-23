using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassNamespaceRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();
    internal string Render()
    {
        sb.AppendLine($"// Auto-generated TypeScript namespace for class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export namespace {symbolNameProvider.GetUserClassNamespace(classInfo)} {{");
        TypescriptUserClassProxyRenderer proxyRenderer = new(classInfo, symbolNameProvider);
        sb.AppendLine(proxyRenderer.Render(depth: 1));
        
        if (classInfo.Properties.Any(p => p.Type.IsSnapshotCompatible))
        {
            TypeScriptUserClassSnapshotRenderer snapshotRenderer = new(classInfo, symbolNameProvider);
            sb.AppendLine(snapshotRenderer.Render(depth: 1));
        }
        sb.AppendLine($"}}");
        return sb.ToString();
    }
}
