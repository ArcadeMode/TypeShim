using Microsoft.CodeAnalysis;
using System.Text;
using TypeShim.Core;
using TypeShim.Generator.Parsing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptUserClassSnapshotRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();
    internal string Render(int depth)
    {
        if (!classInfo.Properties.Any(p => p.Type.IsSnapshotCompatible))
            return string.Empty;

        string indent = new(' ', depth * 2);
        sb.AppendLine($"{indent}export interface {symbolNameProvider.GetSnapshotDefinitionName()} {{");
        RenderInterfaceProperties(depth + 1);
        sb.AppendLine($"{indent}}}");

        RenderInstanceOfRuntimeCheck(depth);

        const string proxyParamName = "proxy";
        sb.AppendLine($"{indent}export function snapshot({proxyParamName}: {symbolNameProvider.GetProxyReferenceName(classInfo)}): {symbolNameProvider.GetSnapshotReferenceName(classInfo)} {{");
        RenderSnapshotFunction(depth + 1, proxyParamName);
        sb.Append($"{indent}}}");
        return sb.ToString();
    }

    private void RenderInterfaceProperties(int depth)
    {
        string indent = new(' ', depth * 2);
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(pi => pi.Type.IsSnapshotCompatible))
        {
            string propertyType = symbolNameProvider.GetSnapshotReferenceNameIfExists(propertyInfo.Type) ?? symbolNameProvider.GetNakedSymbolReference(propertyInfo.Type);
            sb.AppendLine($"{indent}{propertyInfo.Name}: {propertyType};");
        }
    }

    private void RenderInstanceOfRuntimeCheck(int depth)
    {
        string indent = new(' ', depth * 2);
        // Emit a runtime value with Symbol.hasInstance so snapshot interfaces can be checked via `instanceof`.
        // It validates that `v` is an object matching the snapshot interface's definition, including nested type/structure checks in property types.

        string snapshotConstName = symbolNameProvider.GetSnapshotDefinitionName();
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

                if (typeInfo.IsArrayType)
                {
                    InteropTypeInfo elementType = typeInfo.TypeArgument ?? throw new ArgumentException("Array element type is not specified");
                    string expectedTypeSymbol = symbolNameProvider.GetSnapshotReferenceNameIfExists(elementType) ?? symbolNameProvider.GetNakedSymbolReference(elementType);
                    yield return $"Array.isArray(o.{propertyName}) && o.{propertyName}.every((e: any) => {GetBooleanTypeAssertion(expectedTypeSymbol, "e")})";
                }
                else if (typeInfo.IsNullableType)
                {
                    InteropTypeInfo innerType = typeInfo.TypeArgument ?? throw new ArgumentException("Nullable's type argument is not specified"); ;
                    string expectedTypeSymbol = symbolNameProvider.GetSnapshotReferenceNameIfExists(innerType) ?? symbolNameProvider.GetNakedSymbolReference(innerType);
                    yield return $"(o.{propertyName} === null || ({GetBooleanTypeAssertion(expectedTypeSymbol, $"o.{propertyName}")}))";
                }
                else if (typeInfo.IsTaskType)
                {
                    // thenable check over instanceof Promise (cross‑realm safe)
                    yield return $"(o.{propertyName} !== null && typeof (o.{propertyName} as any).then === 'function')";
                }
                else // Simple type or snapshot
                {
                    string expectedTypeSymbol = symbolNameProvider.GetSnapshotReferenceNameIfExists(typeInfo) ?? symbolNameProvider.GetNakedSymbolReference(typeInfo);
                    yield return $"({GetBooleanTypeAssertion(expectedTypeSymbol, $"o.{propertyName}")})";
                }
            }
        }

        string GetBooleanTypeAssertion(string symbol, string referenceExpression)
        {
            return RequiresTypeofExpression(symbol)
                ? $"typeof {referenceExpression} === '{symbol}'"
                : $"{referenceExpression} instanceof {symbol}";
        }

        bool RequiresTypeofExpression(string symbol)
        {
            return symbol == "string" || symbol == "number" || symbol == "boolean" || symbol == "bigint" || symbol == "symbol" || symbol == "undefined";
        }
    }

    private void RenderSnapshotFunction(int depth, string proxyParamName)
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

        string GetSnapshotExpression(InteropTypeInfo typeInfo, string propertyAccessorExpression)
        {
            if (typeInfo.IsArrayType && typeInfo.RequiresCLRTypeConversion)
            {
                InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Array type must have a type argument.");
                return $"{propertyAccessorExpression}.map(item => {GetSnapshotExpression(elementTypeInfo, "item")})";
            }
            else if (typeInfo.IsNullableType)
            {
                InteropTypeInfo innerTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument.");
                return $"{propertyAccessorExpression} ? {GetSnapshotExpression(innerTypeInfo, propertyAccessorExpression)} : null";
            }
            else if (typeInfo.RequiresCLRTypeConversion)
            {
                string userClassName = symbolNameProvider.GetNakedSymbolReference(typeInfo);
                return $"{userClassName}.snapshot({propertyAccessorExpression})";
            }
            else // simple type
            {
                return propertyAccessorExpression;
            }
        }
    }
}