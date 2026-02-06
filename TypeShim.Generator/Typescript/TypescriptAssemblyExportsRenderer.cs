using System.Collections.Generic;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders a TypeScript interface matching the JSExport-generated WebAssembly module exports.
/// </summary>
internal sealed class TypescriptAssemblyExportsRenderer(
    ModuleHierarchyInfo moduleHierarchyInfo,
    RenderContext ctx)
{
    internal void Render()
    {
        ctx.AppendLine("// TypeShim generated TypeScript module exports interface")
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
        foreach (MethodInfo methodInfo in GetAllMethods(classInfo))
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

            ctx.Append(parameterInfo.Name).Append(": ");
            if (parameterInfo.Type.IsDelegateType())
            {
                TypeScriptSymbolNameRenderer.RenderDelegate(parameterInfo.Type, ctx, parameterSymbolType: TypeShimSymbolType.Proxy, returnSymbolType: TypeShimSymbolType.Proxy, interop: true);
            }
            else
            {
                // TODO: remove IsInjectedInstanceParameter property and split into separate methodInfo property
                TypeShimSymbolType symbolType = parameterInfo.IsInjectedInstanceParameter ? TypeShimSymbolType.Proxy : TypeShimSymbolType.ProxyInitializerUnion;
                TypeScriptSymbolNameRenderer.Render(parameterInfo.Type, ctx, symbolType, interop: true);
                //ctx.Append(typeName);
            }
            isFirst = false;
        }
        ctx.Append("): ");

        if (returnType.IsDelegateType())
        {
            TypeScriptSymbolNameRenderer.RenderDelegate(returnType, ctx, parameterSymbolType: TypeShimSymbolType.ProxyInitializerUnion, returnSymbolType: TypeShimSymbolType.Proxy, interop: true);
        }
        else
        {
            TypeScriptSymbolNameRenderer.Render(returnType, ctx, TypeShimSymbolType.Proxy, interop: true);
            //ctx.Append(returnTypeName);
        }
        ctx.AppendLine(";");
    }

    private static IEnumerable<MethodInfo> GetAllMethods(ClassInfo classInfo)
    {
        foreach (MethodInfo methodInfo in classInfo.Methods.Select(m => m))
        {
            yield return methodInfo;
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Select(p => p))
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
