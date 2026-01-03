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
                    string snapshotSymbolName = ctx.SymbolMap.GetUserClassSymbolName(propertyInfo.Type, TypeShimSymbolType.Snapshot);
                    ctx.Append(snapshotSymbolName);
                }
                else
                {
                    ctx.Append(propertyInfo.Type.TypeScriptTypeSyntax.Render());
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
                    ClassInfo classInfo = ctx.SymbolMap.GetClassInfo(propertyInfo.Type.GetInnermostType());
                    TypeShimSymbolType symbolType = classInfo is { Constructor: { IsParameterless: true, AcceptsInitializer: true } }
                        ? TypeShimSymbolType.ProxyInitializerUnion
                        : TypeShimSymbolType.Proxy;
                    ctx.Append(ctx.SymbolMap.GetUserClassSymbolName(classInfo, propertyInfo.Type, symbolType));
                }
                else
                {
                    ctx.Append(propertyInfo.Type.TypeScriptTypeSyntax.Render());
                }
                ctx.AppendLine(";");
            }
        }
        ctx.AppendLine("}");
    }

    internal void RenderPropertiesFunction(string proxyParamName)
    {
        string paramType = ctx.SymbolMap.GetUserClassSymbolName(ctx.Class, TypeShimSymbolType.Proxy);
        string returnType = ctx.SymbolMap.GetUserClassSymbolName(ctx.Class, TypeShimSymbolType.Snapshot);
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
                foreach (PropertyInfo propertyInfo in ctx.Class.Properties.Where(p => !p.IsStatic))
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
                    string tsNamespace = ctx.SymbolMap.GetUserClassSymbolName(typeInfo, TypeShimSymbolType.Namespace);
                    return $"{tsNamespace}.{RenderConstants.ProxyMaterializeFunction}({propertyAccessorExpression})";
                }
            }
            else // simple primitive or unconvertable class
            {
                return propertyAccessorExpression;
            }
        }
    }
}