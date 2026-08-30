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
            // Throws NotSupportedTypeException for unsupported underlying types (long/ulong).
            Type = new InteropTypeInfoBuilder(enumSymbol, typeInfoCache).Build(),
            UnderlyingType = enumSymbol.EnumUnderlyingType?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "int",
            Members = BuildMembers(),
            Comment = new CommentInfoBuilder(enumSymbol).Build(),
        };
    }

    private List<EnumMemberInfo> BuildMembers()
    {
        List<EnumMemberInfo> members = [];
        foreach (IFieldSymbol fieldSymbol in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!fieldSymbol.IsConst || fieldSymbol.ConstantValue is null)
            {
                continue; // skip the synthesized value__ instance field
            }

            members.Add(new EnumMemberInfo
            {
                Name = fieldSymbol.Name,
                Value = Convert.ToInt64(fieldSymbol.ConstantValue),
                Comment = new CommentInfoBuilder(fieldSymbol).Build(),
            });
        }

        return members;
    }
}
