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

        RenderPropertiesInterface();
        RenderInitializerInterface();

        const string proxyParamName = "proxy";
        RenderPropertiesFunction(proxyParamName);
    }

    private void RenderPropertiesInterface()
    {
        PropertyInfo[] propertyInfos = [.. ctx.Class.Properties.Where(p => !p.IsStatic)];

        if (propertyInfos.Length == 0)
            return;

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

    private void RenderInitializerInterface()
    {
        PropertyInfo[]? propertyInfos = ctx.Class.Constructor?.MemberInitializers;
        if (propertyInfos?.Length is 0 or null)
            return;

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
            else if (typeInfo.RequiresTypeConversion)
            {
                if (typeInfo.IsArrayType || typeInfo.IsTaskType)
                {
                    InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Conversion-requiring array/task type must have a type argument.");
                    string transformFunction = typeInfo.IsArrayType ? "map" : "then";
                    return $"{propertyAccessorExpression}.{transformFunction}(e => {GetPropertyValueExpression(elementTypeInfo, "e")})";
                }
                else // simple user type
                {
                    string userClassName = symbolNameProvider.GetNakedSymbolReference(typeInfo);
                    return $"{userClassName}.properties({propertyAccessorExpression})";
                }
            }
            else // simple primitive
            {
                return propertyAccessorExpression;
            }
        }
    }
}