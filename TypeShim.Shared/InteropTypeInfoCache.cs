using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypeShim.Shared;

public sealed class InteropTypeInfoCache
{
    private readonly Dictionary<ITypeSymbol, InteropTypeInfo> cache = new(SymbolEqualityComparer.Default);
    public InteropTypeInfo GetOrAdd(ITypeSymbol typeSymbol, Func<InteropTypeInfo> factory)
    {
        if (!cache.TryGetValue(typeSymbol, out InteropTypeInfo? typeInfo))
        {
            typeInfo = factory();
            cache[typeSymbol] = typeInfo;
        }

        return typeInfo;
    }
}
