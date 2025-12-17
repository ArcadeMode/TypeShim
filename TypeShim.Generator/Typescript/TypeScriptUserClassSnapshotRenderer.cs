using Microsoft.CodeAnalysis;
using System.Text;
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
        // It validates that `v` is an object and that all snapshot-compatible properties exist,
        // and performs array and nested snapshot checks where applicable.

        string snapshotConstName = symbolNameProvider.GetSnapshotDefinitionName();
        sb.AppendLine($"{indent}export const {snapshotConstName}: {{");
        sb.AppendLine($"{indent}  [Symbol.hasInstance](v: unknown): boolean;");
        sb.AppendLine($"{indent}}} = {{");
        sb.AppendLine($"{indent}  [Symbol.hasInstance](v: unknown) {{");
        sb.AppendLine($"{indent}    if (!v || typeof v !== 'object') return false;");
        sb.AppendLine($"{indent}    const o = v as any;");

        List<string> checks = new();
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(pi => pi.Type.IsSnapshotCompatible))
        {
            InteropTypeInfo typeInfo = propertyInfo.Type;
            string prop = propertyInfo.Name;

            if (typeInfo.IsArrayType)
            {
                InteropTypeInfo elementType = typeInfo.TypeArgument ?? throw new ArgumentException("Array element type is not specified");
                string targetSnapshot = symbolNameProvider.GetSnapshotReferenceNameIfExists(elementType) ?? symbolNameProvider.GetNakedSymbolReference(elementType);
                checks.Add($"Array.isArray(o.{prop}) && o.{prop}.every(e => e instanceof {targetSnapshot})");
            }
            else if (typeInfo.IsNullableType)
            {
                InteropTypeInfo innerType = typeInfo.TypeArgument ?? throw new ArgumentException("Nullable's type argument is not specified"); ;
                string targetSnapshot = symbolNameProvider.GetSnapshotReferenceNameIfExists(innerType) ?? symbolNameProvider.GetNakedSymbolReference(innerType);
                checks.Add($"(o.{prop} === null || (o.{prop} instanceof {targetSnapshot}))");
            }
            else if (typeInfo.IsTaskType)
            {
                // thenable check over instanceof Promise (cross‑realm safe)
                checks.Add($"(o.{prop} !== null && typeof (o.{prop} as any).then === 'function')");
            }
            else // Simple type or snapshot
            {
                string targetSnapshot = symbolNameProvider.GetSnapshotReferenceNameIfExists(typeInfo) ?? symbolNameProvider.GetNakedSymbolReference(typeInfo);
                checks.Add($"(o.{prop} instanceof {targetSnapshot})");
            }
        }

        if (checks.Count > 0)
        {
            sb.AppendLine($"{indent}    return " + string.Join(" && ", checks) + ";");
        }
        else
        {
            sb.AppendLine($"{indent}    return true;");
        }

        sb.AppendLine($"{indent}  }}");
        sb.AppendLine($"{indent}}};");
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