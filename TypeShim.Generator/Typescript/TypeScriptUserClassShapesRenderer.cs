using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassShapesRenderer(TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    internal void Render()
    {
        if (ctx.Class.IsStatic) return;

        PropertyInfo[] instancePropertyInfos = [.. ctx.Class.Properties.Where(p => !p.IsStatic)];
        if (instancePropertyInfos.Length == 0)
            return;

        if (ctx.Class.Constructor?.MemberInitializers is { Length: > 0 } initializerPropertyInfos)
        {
            RenderInitializerInterface(initializerPropertyInfos);
        }

        RenderPropertiesInterface(instancePropertyInfos);
        const string proxyParamName = "proxy";
        RenderPropertiesFunction(proxyParamName);
    }

    private void RenderPropertiesInterface(PropertyInfo[] propertyInfos)
    {
        ctx.Append($"export interface ").Append(RenderConstants.Properties).AppendLine(" {");
        using (ctx.Indent())
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string propertyType = symbolNameProvider.GetUserClassSymbolNameIfExists(propertyInfo.Type, SymbolNameFlags.Properties) ?? symbolNameProvider.GetNakedSymbolReference(propertyInfo.Type);
                ctx.Append(propertyInfo.Name).Append(": ").Append(propertyType).AppendLine(";");
            }
        }
        ctx.AppendLine("}");
    }

    private void RenderInitializerInterface(PropertyInfo[] propertyInfos)
    {
        ctx.Append($"export interface ").Append(RenderConstants.Initializer).AppendLine(" {");
        using (ctx.Indent())
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string propertyType = symbolNameProvider.GetUserClassSymbolNameIfExists(propertyInfo.Type, SymbolNameFlags.ProxyInitializerUnion) ?? symbolNameProvider.GetNakedSymbolReference(propertyInfo.Type);
                ctx.Append(propertyInfo.Name).Append(": ").Append(propertyType).AppendLine(";");
            }
        }
        ctx.AppendLine("}");
    }

    private void RenderPropertiesFunction(string proxyParamName)
    {
        string paramType = symbolNameProvider.GetUserClassSymbolName(ctx.Class, RenderConstants.Proxy);
        string returnType = symbolNameProvider.GetUserClassSymbolName(ctx.Class, RenderConstants.Properties);
        ctx.Append($"export function ").Append(RenderConstants.PropertiesTSFunction).Append('(').Append(proxyParamName).Append(": ").Append(paramType).Append("): ").Append(returnType).AppendLine(" {");
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
                    ClassInfo exportedClass = ctx.GetClassInfo(typeInfo);
                    string userClassName = symbolNameProvider.GetUserClassSymbolName(exportedClass);
                    return $"{userClassName}.{RenderConstants.PropertiesTSFunction}({propertyAccessorExpression})";
                }
            }
            else // simple primitive or unconvertable class
            {
                return propertyAccessorExpression;
            }
        }
    }
}