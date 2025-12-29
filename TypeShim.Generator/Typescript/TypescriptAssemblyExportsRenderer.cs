using System.Collections.Generic;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the JSExport-generated WebAssembly module exports.
/// </summary>
internal sealed class TypescriptAssemblyExportsRenderer(
    ModuleHierarchyInfo moduleHierarchyInfo,
    TypescriptSymbolNameProvider symbolNameProvider,
    RenderContext ctx)
{
    internal string Render()
    {
        ctx.AppendLine("// Auto-generated TypeScript module exports interface")
           .Append("export interface ")
           .Append(symbolNameProvider.GetModuleInteropClassName())
           .AppendLine("{");

        using (ctx.Indent())
        {
            RenderModuleInfoObject(moduleHierarchyInfo);
        }

        ctx.AppendLine("}");
        return ctx.Render();
    }

    private void RenderModuleInfoObject(ModuleHierarchyInfo moduleInfo)
    {
        foreach (KeyValuePair<string, ModuleHierarchyInfo> child in moduleInfo.Children)
        {
            ctx.Append(child.Key).AppendLine(": {");
            using (ctx.Indent())
            {
                if (child.Value.ExportedClass is ClassInfo classInfo)
                {
                    RenderClassInteropMethods(classInfo);
                }
                else
                {
                    RenderModuleInfoObject(child.Value); // recurse into child object
                }
            }
            ctx.AppendLine("};");
        }
    }

    private void RenderClassInteropMethods(ClassInfo classInfo)
    {
        foreach (MethodInfo methodInfo in GetAllMethods(classInfo))
        {
            RenderInteropMethodSignature(methodInfo);
        }
    }

    private void RenderInteropMethodSignature(MethodInfo methodInfo)
    {
        ctx.Append(methodInfo.Name).Append('(');
        bool isFirst = true;
        foreach (MethodParameterInfo parameterInfo in methodInfo.Parameters)
        {
            if (!isFirst) ctx.Append(", ");

            ctx.Append(parameterInfo.Name).Append(": ").Append(symbolNameProvider.GetNakedSymbolReference(parameterInfo.Type));
            isFirst = false;
        }
        ctx.Append("): ").Append(symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType)).AppendLine(";");
    }

    private static IEnumerable<MethodInfo> GetAllMethods(ClassInfo classInfo)
    {
        foreach (MethodInfo methodInfo in classInfo.Methods.Select(m => m.WithInteropTypeInfo()))
        {
            yield return methodInfo;
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Select(p => p.WithInteropTypeInfo()))
        {
            yield return propertyInfo.GetMethod;

            if (propertyInfo.SetMethod is not MethodInfo setMethod)
            {
                continue;
            }
            yield return setMethod;
            // Note: init is not rendered as an interop method.
        }
    }
}
