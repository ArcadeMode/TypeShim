using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace TypeScriptExportGenerator;

public static class ISymbolConverter
{
    // symbol: should be an INamedTypeSymbol representing a class
    public static string ConvertClassToTypeScript(INamedTypeSymbol symbol)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"export interface {symbol.Name} {{");

        // Public properties
        foreach (var member in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.DeclaredAccessibility == Accessibility.Public)
            {
                sb.AppendLine($"  {member.Name}: {MapCSharpTypeToTypeScript(member.Type)};");
            }
        }

        // Public fields
        foreach (var member in symbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.DeclaredAccessibility == Accessibility.Public)
            {
                sb.AppendLine($"  {member.Name}: {MapCSharpTypeToTypeScript(member.Type)};");
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // Simple type map, extend as needed
    private static string MapCSharpTypeToTypeScript(ITypeSymbol type)
    {
        if (type == null)
            return "any";

        switch (type.SpecialType)
        {
            case SpecialType.System_String: 
                return "string";
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt16:
            case SpecialType.System_UInt32:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return "number";
            case SpecialType.System_Boolean: 
                return "boolean";
            case SpecialType.System_Object: 
                return "any";
            default:
                // Array types
                if (type is IArrayTypeSymbol ats)
                    return MapCSharpTypeToTypeScript(ats.ElementType) + "[]";
                // Nullable types
                if (type is INamedTypeSymbol nts && nts.IsGenericType &&
                    nts.Name == "Nullable" && nts.TypeArguments.Length == 1)
                    return MapCSharpTypeToTypeScript(nts.TypeArguments[0]) + " | null";
                // Enum types
                if (type.TypeKind == TypeKind.Enum)
                    return "number";
                // Other classes/interfaces
                return type.Name;
        }
    }
}