using Microsoft.CodeAnalysis;
using TypeShim.Shared;

namespace TypeShim.Generator.Parsing;

/// <summary>
/// Resolves an exported symbol into the appropriate <see cref="NamedTypeInfo"/> (class or enum),
/// or <c>null</c> when the symbol should not be projected.
/// </summary>
internal sealed class NamedTypeInfoBuilder(INamedTypeSymbol symbol, InteropTypeInfoCache typeInfoCache)
{
    internal NamedTypeInfo? Build()
    {
        return symbol.TypeKind switch
        {
            TypeKind.Class => BuildClass(),
            // Only [TSExport] enums survive symbol extraction, so any enum here is projected.
            TypeKind.Enum => new EnumInfoBuilder(symbol, typeInfoCache).Build(),
            _ => null,
        };
    }

    private ClassInfo? BuildClass()
    {
        ClassInfo classInfo = new ClassInfoBuilder(symbol, typeInfoCache).Build();
        // dont bother with empty classes
        return classInfo.Methods.Any() || classInfo.Properties.Any() ? classInfo : null;
    }
}
