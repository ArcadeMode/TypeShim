using System;
using Microsoft.CodeAnalysis;
using TypeShim.Shared;

namespace TypeShim.Generator.Parsing;

internal sealed class EnumInfoBuilder(INamedTypeSymbol enumSymbol, InteropTypeInfoCache typeInfoCache)
{
    internal EnumInfo Build()
    {
        return new EnumInfo
        {
            Namespace = enumSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Name = enumSymbol.Name,
            // Throws NotSupportedTypeException for unsupported underlying types (ulong).
            Type = new InteropTypeInfoBuilder(enumSymbol, typeInfoCache).Build(),
            UnderlyingType = enumSymbol.EnumUnderlyingType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "int",
            Members = BuildMembers(),
            Comment = new CommentInfoBuilder(enumSymbol).Build(),
        };
    }

    // JS numbers represent integers exactly only within +/-(2^53 - 1).
    private const long MaxSafeInteger = 9007199254740991;

    private List<EnumMemberInfo> BuildMembers()
    {
        List<EnumMemberInfo> members = [];
        foreach (IFieldSymbol fieldSymbol in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!fieldSymbol.IsConst || fieldSymbol.ConstantValue is null)
            {
                continue; // skip the synthesized value__ instance field
            }

            long value = Convert.ToInt64(fieldSymbol.ConstantValue);
            if (value > MaxSafeInteger || value < -MaxSafeInteger)
            {
                throw new NotSupportedTypeException(
                    $"Enum member '{enumSymbol.Name}.{fieldSymbol.Name}' has value {value}, which is outside the JS safe-integer range (+/-2^53-1) and cannot be represented exactly.");
            }

            members.Add(new EnumMemberInfo
            {
                Name = fieldSymbol.Name,
                Value = value,
                Comment = new CommentInfoBuilder(fieldSymbol).Build(),
            });
        }

        return members;
    }
}
