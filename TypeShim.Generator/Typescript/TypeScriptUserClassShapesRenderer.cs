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
                    TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx, TypeShimSymbolType.Snapshot, interop: false);
                }
                else
                {
                    TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx);
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
                        TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx, TypeShimSymbolType.ProxyInitializerUnion, interop: false);
                    }
                }
                else
                {
                    TypeScriptSymbolNameRenderer.Render(propertyInfo.Type, ctx);
                }
                ctx.AppendLine(";");
            }
        }
        ctx.AppendLine("}");
    }

    internal void RenderPropertiesFunction(string proxyParamName)
    {
        ctx.Append($"export function ").Append(RenderConstants.ProxyMaterializeFunction).Append('(').Append(proxyParamName).Append(": ");
        TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Proxy, interop: false);
        ctx.Append("): ");
        TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Snapshot, interop: false);
        ctx.AppendLine(" {");
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
                    ctx.Append(propertyInfo.Name).Append(": ");
                    RenderPropertyValueExpression(propertyInfo.Type, $"{proxyParamName}.{propertyInfo.Name}");
                    ctx.AppendLine(",");
                }
            }
            ctx.AppendLine("};");
        }

        // TODO: refactor to RenderPropertyValueExpression (use ctx)
        void RenderPropertyValueExpression(InteropTypeInfo typeInfo, string propertyAccessorExpression) 
        {
            if (typeInfo.IsNullableType)
            {
                InteropTypeInfo innerTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument.");
                ctx.Append(propertyAccessorExpression).Append(" ? ");
                RenderPropertyValueExpression(innerTypeInfo, propertyAccessorExpression);
                ctx.Append(" : null");
            }
            else if (typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion)
            {
                if (typeInfo.IsArrayType || typeInfo.IsTaskType)
                {
                    InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Conversion-requiring array/task type must have a type argument.");
                    string transformFunction = typeInfo.IsArrayType ? "map" : "then";
                    ctx.Append(propertyAccessorExpression).Append('.').Append(transformFunction).Append("(e => ");
                    RenderPropertyValueExpression(elementTypeInfo, "e");
                    ctx.Append(')');
                }
                else // exported user type
                {
                    TypeScriptSymbolNameRenderer.Render(typeInfo, ctx, TypeShimSymbolType.Namespace, interop: false);
                    ctx.Append('.').Append(RenderConstants.ProxyMaterializeFunction).Append('(').Append(propertyAccessorExpression).Append(')');
                }
            }
            else // simple primitive or unconvertable class
            {
                ctx.Append(propertyAccessorExpression);
            }
        }
    }
}