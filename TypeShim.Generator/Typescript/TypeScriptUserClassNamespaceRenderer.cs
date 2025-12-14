using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptUserClassNamespaceRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();
    internal string Render()
    {
        TypescriptUserClassProxyRenderer proxyRenderer = new(classInfo, methodRenderer, symbolNameProvider);

        sb.AppendLine($"// Auto-generated TypeScript namespace for class: {classInfo.Namespace}.{classInfo.Name}");
        sb.AppendLine($"export namespace {symbolNameProvider.GetUserClassNamespace(classInfo)} {{");
        sb.AppendLine(proxyRenderer.Render(depth: 1));
        sb.AppendLine($"}}");
        return sb.ToString();
    }
}
