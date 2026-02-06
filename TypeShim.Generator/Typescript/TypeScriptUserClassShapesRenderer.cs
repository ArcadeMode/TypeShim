using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassShapesRenderer(RenderContext ctx)
{
    internal void RenderPropertiesInterface(PropertyInfo[] propertyInfos)
    {
        ctx.Append($"export interface ").Append(RenderConstants.Properties).AppendLine(" {");
        using (ctx.Indent())
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                ctx.Append(propertyInfo.Name).Append(": ");
                if (propertyInfo.Type is { RequiresTypeConversion: true, SupportsTypeConversion: true })
                {
                    string snapshotSymbolName = TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx, TypeShimSymbolType.Snapshot, interop: false);
                    ctx.Append(snapshotSymbolName);
                }
                else
                {
                    ctx.Append(TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx));
                }
                ctx.AppendLine(";");
            }
        }
        ctx.AppendLine("}");
    }

    internal void RenderInitializerInterface(PropertyInfo[] propertyInfos)
    {
        ctx.Append($"export interface ").Append(RenderConstants.Initializer).AppendLine(" {");
        using (ctx.Indent())
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                ctx.Append(propertyInfo.Name).Append(": ");
                if (propertyInfo.Type is { RequiresTypeConversion: true, SupportsTypeConversion: true })
                {
                    if (propertyInfo.Type.IsDelegateType())
                    {
                        TypeScriptSymbolNameRenderer.RenderDelegate(propertyInfo.Type, ctx, parameterSymbolType: TypeShimSymbolType.ProxyInitializerUnion, returnSymbolType: TypeShimSymbolType.Proxy, interop: false);
                    }
                    else
                    {
                        string symbolName = TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx, TypeShimSymbolType.ProxyInitializerUnion, interop: false);
                        ctx.Append(symbolName);
                    }
                }
                else
                {
                    ctx.Append(TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx));
                }
                ctx.AppendLine(";");
            }
        }
        ctx.AppendLine("}");
    }

    internal void RenderPropertiesFunction(string proxyParamName)
    {
        string paramType = TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Proxy, interop: false);
        string returnType = TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Snapshot, interop: false);
        ctx.Append($"export function ").Append(RenderConstants.ProxyMaterializeFunction).Append('(').Append(proxyParamName).Append(": ").Append(paramType).Append("): ").Append(returnType).AppendLine(" {");
        using (ctx.Indent())
        {
            RenderFunctionBody(proxyParamName);
        }
        ctx.AppendLine("}");

        void RenderFunctionBody(string proxyParamName)
        {
            ctx.AppendLine("return {");
            using (ctx.Indent())
            {
                foreach (PropertyInfo propertyInfo in ctx.Class.Properties.Where(p => !p.IsStatic && !p.Type.IsDelegateType()))
                {
                    ctx.Append(propertyInfo.Name).Append(": ")
                       .Append(GetPropertyValueExpression(propertyInfo.Type, $"{proxyParamName}.{propertyInfo.Name}"))
                       .AppendLine(",");
                }
            }
            ctx.AppendLine("};");
        }

        // TODO: refactor to RenderPropertyValueExpression (use ctx)
        string GetPropertyValueExpression(InteropTypeInfo typeInfo, string propertyAccessorExpression) 
        {
            if (typeInfo.IsNullableType)
            {
                InteropTypeInfo innerTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument.");
                return $"{propertyAccessorExpression} ? {GetPropertyValueExpression(innerTypeInfo, propertyAccessorExpression)} : null";
            }
            else if (typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion)
            {
                if (typeInfo.IsArrayType || typeInfo.IsTaskType)
                {
                    InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Conversion-requiring array/task type must have a type argument.");
                    string transformFunction = typeInfo.IsArrayType ? "map" : "then";
                    return $"{propertyAccessorExpression}.{transformFunction}(e => {GetPropertyValueExpression(elementTypeInfo, "e")})";
                }
                else // exported user type
                {
                    string tsNamespace = TypeScriptSymbolNameRenderer.Render(typeInfo, ctx, TypeShimSymbolType.Namespace, interop: false);
                    return $"{tsNamespace}.{RenderConstants.ProxyMaterializeFunction}({propertyAccessorExpression})";
                }
            }
            else // simple primitive or unconvertable class
            {
                return propertyAccessorExpression;
            }

            void RenderTypeConversionForMaterialization(InteropTypeInfo typeInfo, Action renderExpression)
            {
                bool requiresProxyConversion = typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion;
                // note we stay on ts side so we need not do char conversion or handle extraction.
                if (requiresProxyConversion && typeInfo.IsDelegateType())
                {
                    // (arg0: x, arg1: y) => 
                    // renderExpression
                    // (arg0, materialize(arg1))
                }
                else if (requiresProxyConversion)
                {
                    // materialize(renderExpression)
                }
                else
                {
                    renderExpression();
                }
            }
        }
    }
}