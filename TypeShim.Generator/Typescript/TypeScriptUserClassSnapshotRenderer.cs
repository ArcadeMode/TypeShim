using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassSnapshotRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();
    internal string Render(int depth)
    {
        if (!classInfo.IsSnapshotCompatible())
            throw new InvalidOperationException($"Type '{classInfo.Namespace}.{classInfo.Name}' is not snapshot-compatible.");

        string indent = new(' ', depth * 2);
        sb.AppendLine($"{indent}export interface {symbolNameProvider.GetUserClassSnapshotSymbolName()} {{");
        RenderInterfaceProperties(depth + 1);
        sb.AppendLine($"{indent}}}");

        RenderInstanceOfRuntimeCheck(depth);

        const string proxyParamName = "proxy";
        RenderSnapshotFunction(depth, proxyParamName);
        return sb.ToString();
    }

    private void RenderInterfaceProperties(int depth)
    {
        string indent = new(' ', depth * 2);
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(pi => pi.Type.IsSnapshotCompatible))
        {
            string propertyType = symbolNameProvider.GetUserClassSymbolNameIfExists(propertyInfo.Type, SymbolNameFlags.Snapshot) ?? symbolNameProvider.GetNakedSymbolReference(propertyInfo.Type);
            sb.AppendLine($"{indent}{propertyInfo.Name}: {propertyType};");
        }
    }

    private void RenderInstanceOfRuntimeCheck(int depth)
    {
        string indent = new(' ', depth * 2);
        // Emit a runtime value with Symbol.hasInstance so snapshot interfaces can be checked via `instanceof`.
        // It validates that `v` is an object matching the snapshot interface's definition, including nested type/structure checks in property types.

        string snapshotConstName = symbolNameProvider.GetUserClassSnapshotSymbolName();
        sb.AppendLine($"{indent}export const {snapshotConstName}: {{");
        sb.AppendLine($"{indent}  [Symbol.hasInstance](v: unknown): boolean;");
        sb.AppendLine($"{indent}}} = {{");
        sb.AppendLine($"{indent}  [Symbol.hasInstance](v: unknown) {{");
        sb.AppendLine($"{indent}    if (!v || typeof v !== 'object') return false;");
        sb.AppendLine($"{indent}    const o = v as any;");

        PropertyInfo[] propertyInfos = [.. classInfo.Properties.Where(pi => pi.Type.IsSnapshotCompatible)];
        string structuralMatchExpression = propertyInfos.Length == 0 ? "true" : string.Join(" && ", GetPropertyTypeAssertionExpressions(propertyInfos));
        sb.AppendLine($"{indent}    return {structuralMatchExpression};");

        sb.AppendLine($"{indent}  }}");
        sb.AppendLine($"{indent}}};");
        return;

        IEnumerable<string> GetPropertyTypeAssertionExpressions(PropertyInfo[] propertyInfos)
        {
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                InteropTypeInfo typeInfo = propertyInfo.Type;
                string propertyName = propertyInfo.Name;
                yield return GetBooleanTypeAssertion(typeInfo, $"o.{propertyName}");
            }
        }

        string GetBooleanTypeAssertion(InteropTypeInfo typeInfo, string referenceExpression)
        {
            if (typeInfo.IsArrayType)
            {
                InteropTypeInfo elementType = typeInfo.TypeArgument ?? throw new ArgumentException("Array element type is not specified");
                return $"Array.isArray({referenceExpression}) && {referenceExpression}.every((e: any) => {GetBooleanTypeAssertion(elementType, "e")})";
            }
            else if(typeInfo.IsNullableType)
            {
                return $"({referenceExpression} === null || {GetBooleanTypeAssertion(typeInfo.TypeArgument!, referenceExpression)})";
            }
            else if(typeInfo.IsTaskType)
            {
                // thenable check over instanceof Promise (cross‑realm safe)
                return $"({referenceExpression} !== null && typeof ({referenceExpression} as any).then === 'function')"; // no recurse, would make typechecking async
            } 
            else
            {
                string symbol = symbolNameProvider.GetUserClassSymbolNameIfExists(typeInfo, SymbolNameFlags.Snapshot | SymbolNameFlags.Isolated) ?? symbolNameProvider.GetNakedSymbolReference(typeInfo);
                return RequiresTypeofExpression(symbol)
                        ? $"typeof {referenceExpression} === '{symbol}'"
                        : $"{referenceExpression} instanceof {symbol}";
            }
        }

        bool RequiresTypeofExpression(string symbol)
        {
            return symbol == "string" || symbol == "number" || symbol == "boolean" || symbol == "bigint" || symbol == "symbol" || symbol == "undefined";
        }
    }


    private void RenderSnapshotFunction(int depth, string proxyParamName)
    {
        string indent = new(' ', depth * 2);
        sb.AppendLine($"{indent}export function snapshot({proxyParamName}: {symbolNameProvider.GetUserClassProxySymbolName(classInfo)}): {symbolNameProvider.GetUserClassSnapshotSymbolName(classInfo)} {{");
        RenderSnapshotFunctionBody(depth + 1, proxyParamName);
        sb.Append($"{indent}}}");

        void RenderSnapshotFunctionBody(int depth, string proxyParamName)
        {
            string indent = new(' ', depth * 2);
            string indent2 = new(' ', (depth + 1) * 2);

            sb.AppendLine($"{indent}return {{");
            foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(pi => pi.Type.IsSnapshotCompatible))
            {
                string propertyAccessorExpression = $"{proxyParamName}.{propertyInfo.Name}";
                sb.AppendLine($"{indent2}{propertyInfo.Name}: {GetSnapshotExpression(propertyInfo.Type, propertyAccessorExpression)},");
            }
            sb.AppendLine($"{indent}}};");
        }

        string GetSnapshotExpression(InteropTypeInfo typeInfo, string propertyAccessorExpression)
        {
            if (typeInfo.IsNullableType)
            {
                InteropTypeInfo innerTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument.");
                return $"{propertyAccessorExpression} ? {GetSnapshotExpression(innerTypeInfo, propertyAccessorExpression)} : null";
            }
            else if (typeInfo.RequiresCLRTypeConversion)
            {
                if (typeInfo.IsArrayType || typeInfo.IsTaskType)
                {
                    InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Conversion-requiring array/task type must have a type argument.");
                    string transformFunction = typeInfo.IsArrayType ? "map" : "then";
                    return $"{propertyAccessorExpression}.{transformFunction}(e => {GetSnapshotExpression(elementTypeInfo, "e")})";
                }
                else // simple user type
                {
                    string userClassName = symbolNameProvider.GetNakedSymbolReference(typeInfo);
                    return $"{userClassName}.snapshot({propertyAccessorExpression})";
                }
            }
            else // simple primitive
            {
                return propertyAccessorExpression;
            }
        }
    }
}