using System.Collections.Generic;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the JSExport-generated WebAssembly module exports.
/// </summary>
internal sealed class TypescriptAssemblyExportsRenderer(
    ModuleHierarchyInfo moduleHierarchyInfo,
    TypescriptSymbolNameProvider symbolNameProvider,
    RenderContext ctx)
{
    internal void Render()
    {
        ctx.AppendLine("// Auto-generated TypeScript module exports interface")
           .Append("export interface ").Append(RenderConstants.AssemblyExports).AppendLine("{");
        using (ctx.Indent())
        {
            RenderModuleInfoObject(moduleHierarchyInfo);
        }
        ctx.AppendLine("}");
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
        if (classInfo.Constructor is ConstructorInfo constructorInfo)
        {
            constructorInfo = constructorInfo.WithInteropTypeInfo();
            RenderInteropMethodSignature(constructorInfo.Name, constructorInfo.GetParametersIncludingInitializerObject(), constructorInfo.Type);
        }
        foreach (MethodInfo methodInfo in GetAllMethodsWithInteropTypeInfo(classInfo))
        {
            RenderInteropMethodSignature(methodInfo.Name, methodInfo.Parameters, methodInfo.ReturnType);
        }
    }

    private void RenderInteropMethodSignature(string name, IEnumerable<MethodParameterInfo> parameters, InteropTypeInfo returnType)
    {
        ctx.Append(name).Append('(');
        bool isFirst = true;
        foreach (MethodParameterInfo parameterInfo in parameters)
        {
            if (!isFirst) ctx.Append(", ");

            ctx.Append(parameterInfo.Name).Append(": ").Append(symbolNameProvider.GetNakedSymbolReference(parameterInfo.Type));
            isFirst = false;
        }
        ctx.Append("): ").Append(symbolNameProvider.GetNakedSymbolReference(returnType)).AppendLine(";");
    }

    private static IEnumerable<MethodInfo> GetAllMethodsWithInteropTypeInfo(ClassInfo classInfo)
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
