using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassSnapshotRenderer(TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    internal void Render()
    {
        throw new InvalidProgramException("OBSOLETE");
        //if (!ctx.Class.IsSnapshotCompatible())
        //    throw new InvalidOperationException($"Type '{ctx.Class.Namespace}.{ctx.Class.Name}' is not snapshot-compatible.");

        //ctx.Append($"export interface ").Append(RenderConstants.Snapshot).AppendLine(" {");
        //using (ctx.Indent())
        //{
        //    RenderInterfaceProperties();

        //}
        //ctx.AppendLine("}");

        //RenderInstanceOfRuntimeCheck();

        //const string proxyParamName = "proxy";
        //RenderSnapshotFunction(proxyParamName);
    }

    //private void RenderInterfaceProperties()
    //{
    //    foreach (PropertyInfo propertyInfo in ctx.Class.Properties.Where(pi => pi.IsSnapshotCompatible()))
    //    {
    //        string propertyType = symbolNameProvider.GetUserClassSymbolNameIfExists(propertyInfo.Type, SymbolNameFlags.Snapshot) ?? symbolNameProvider.GetNakedSymbolReference(propertyInfo.Type);
    //        ctx.Append(propertyInfo.Name).Append(": ").Append(propertyType).AppendLine(";");
    //    }
    //}

    //private void RenderInstanceOfRuntimeCheck()
    //{
    //    // Emit a runtime value with Symbol.hasInstance so snapshot interfaces can be checked via `instanceof`.
    //    // It validates that `v` is an object matching the snapshot interface's _structure_, including nested typechecks.

    //    ctx.Append($"export const ").Append(RenderConstants.Snapshot).AppendLine(": {");
    //    using (ctx.Indent())
    //    {
    //        ctx.AppendLine("[Symbol.hasInstance](v: unknown): boolean;");
    //    }
    //    ctx.AppendLine("} = {");
    //    using (ctx.Indent())
    //    {
    //        ctx.AppendLine("[Symbol.hasInstance](v: unknown) {");
    //        using (ctx.Indent())
    //        {
    //            ctx.AppendLine($"if (!v || typeof v !== 'object') return false;");
    //            ctx.AppendLine($"const o = v as any;");

    //            PropertyInfo[] propertyInfos = [.. ctx.Class.Properties.Where(pi => pi.IsSnapshotCompatible())];
    //            string structuralMatchExpression = propertyInfos.Length == 0 ? "true" : string.Join(" && ", GetPropertyTypeAssertionExpressions(propertyInfos));
    //            ctx.Append("return ").Append(structuralMatchExpression).AppendLine(";");
    //        }
    //        ctx.AppendLine("}");
    //    }
    //    ctx.AppendLine("};");
    //    return;

    //    // TODO: refactor to RenderPropertyTypeAssertionExpressions (use ctx)
    //    IEnumerable<string> GetPropertyTypeAssertionExpressions(PropertyInfo[] propertyInfos)
    //    {
    //        foreach (PropertyInfo propertyInfo in propertyInfos)
    //        {
    //            InteropTypeInfo typeInfo = propertyInfo.Type;
    //            string propertyName = propertyInfo.Name;
    //            yield return GetBooleanTypeAssertion(typeInfo, $"o.{propertyName}");
    //        }
    //    }

    //    // TODO: refactor to RenderBooleanTypeAssertion (use ctx)
    //    string GetBooleanTypeAssertion(InteropTypeInfo typeInfo, string referenceExpression)
    //    {
    //        if (typeInfo.IsArrayType)
    //        {
    //            InteropTypeInfo elementType = typeInfo.TypeArgument ?? throw new ArgumentException("Array element type is not specified");
    //            return $"Array.isArray({referenceExpression}) && {referenceExpression}.every((e: any) => {GetBooleanTypeAssertion(elementType, "e")})";
    //        }
    //        else if(typeInfo.IsNullableType)
    //        {
    //            return $"({referenceExpression} === null || {GetBooleanTypeAssertion(typeInfo.TypeArgument!, referenceExpression)})";
    //        }
    //        else if(typeInfo.IsTaskType)
    //        {
    //            // thenable check over instanceof Promise (cross‑realm safe)
    //            return $"({referenceExpression} !== null && typeof ({referenceExpression} as any).then === 'function')"; // no recurse, would make typechecking async
    //        } 
    //        else
    //        {
    //            string symbol = symbolNameProvider.GetUserClassSymbolNameIfExists(typeInfo, SymbolNameFlags.Snapshot | SymbolNameFlags.Isolated) ?? symbolNameProvider.GetNakedSymbolReference(typeInfo);
    //            return RequiresTypeofExpression(symbol)
    //                    ? $"typeof {referenceExpression} === '{symbol}'"
    //                    : $"{referenceExpression} instanceof {symbol}";
    //        }
    //    }

    //    bool RequiresTypeofExpression(string symbol)
    //    {
    //        return symbol == "string" || symbol == "number" || symbol == "boolean" || symbol == "bigint" || symbol == "symbol" || symbol == "undefined";
    //    }
    //}


    //private void RenderSnapshotFunction(string proxyParamName)
    //{
    //    string paramType = symbolNameProvider.GetUserClassProxySymbolName(ctx.Class);
    //    string returnType = symbolNameProvider.GetUserClassSnapshotSymbolName(ctx.Class);
    //    ctx.Append($"export function snapshot(").Append(proxyParamName).Append(": ").Append(paramType).Append("): ").Append(returnType).AppendLine(" {");
    //    using (ctx.Indent())
    //    {
    //        RenderSnapshotFunctionBody(proxyParamName);
    //    }
    //    ctx.AppendLine("}");

    //    void RenderSnapshotFunctionBody(string proxyParamName)
    //    {
    //        ctx.AppendLine("return {");
    //        using (ctx.Indent())
    //        {
    //            foreach (PropertyInfo propertyInfo in ctx.Class.Properties.Where(pi => pi.IsSnapshotCompatible()))
    //            {
    //                ctx.Append(propertyInfo.Name).Append(": ")
    //                   .Append(GetSnapshotExpression(propertyInfo.Type, $"{proxyParamName}.{propertyInfo.Name}"))
    //                   .AppendLine(",");
    //            }
    //        }
    //        ctx.AppendLine("};");
    //    }

    //    // TODO: refactor to RenderSnapshotExpression (use ctx)
    //    string GetSnapshotExpression(InteropTypeInfo typeInfo, string propertyAccessorExpression) 
    //    {
    //        if (typeInfo.IsNullableType)
    //        {
    //            InteropTypeInfo innerTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument.");
    //            return $"{propertyAccessorExpression} ? {GetSnapshotExpression(innerTypeInfo, propertyAccessorExpression)} : null";
    //        }
    //        else if (typeInfo.RequiresTypeConversion)
    //        {
    //            if (typeInfo.IsArrayType || typeInfo.IsTaskType)
    //            {
    //                InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Conversion-requiring array/task type must have a type argument.");
    //                string transformFunction = typeInfo.IsArrayType ? "map" : "then";
    //                return $"{propertyAccessorExpression}.{transformFunction}(e => {GetSnapshotExpression(elementTypeInfo, "e")})";
    //            }
    //            else // simple user type
    //            {
    //                string userClassName = symbolNameProvider.GetNakedSymbolReference(typeInfo);
    //                return $"{userClassName}.snapshot({propertyAccessorExpression})";
    //            }
    //        }
    //        else // simple primitive
    //        {
    //            return propertyAccessorExpression;
    //        }
    //    }
    //}
}