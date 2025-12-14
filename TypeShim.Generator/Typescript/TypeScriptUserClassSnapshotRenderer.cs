using Microsoft.CodeAnalysis;
using System.Text;
using TypeShim.Generator.Parsing;

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