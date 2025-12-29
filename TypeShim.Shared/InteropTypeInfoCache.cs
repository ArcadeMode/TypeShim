using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace TypeShim.Shared;

public sealed class InteropTypeInfoCache
{
    private readonly Dictionary<ITypeSymbol, InteropTypeInfo> _cache = new(SymbolEqualityComparer.IncludeNullability);
    public InteropTypeInfo GetOrAdd(ITypeSymbol typeSymbol, Func<InteropTypeInfo> factory)
    {
        if (typeSymbol.IsDefinition)
        {
            // Consider type definitions to be not annotated like references to these types are,
            // fixes cache misses and issues in type identity downstream.
            typeSymbol = typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        }
        if (!_cache.TryGetValue(typeSymbol, out InteropTypeInfo? typeInfo))
        {
            typeInfo = factory();
            _cache.Add(typeSymbol, typeInfo);
        }
        
        return typeInfo;
    }
}
