using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class SymbolMap(IEnumerable<NamedTypeInfo> allNamedTypes)
{
    private readonly Dictionary<InteropTypeInfo, NamedTypeInfo> _typeToNamedTypeDict = allNamedTypes.ToDictionary(n => n.Type);

    internal ClassInfo GetClassInfo(InteropTypeInfo type)
    {
        _typeToNamedTypeDict.TryGetValue(type, out NamedTypeInfo? info);
        return info as ClassInfo ?? throw new NotFoundClassInfoException($"Could not find ClassInfo for type: {type.CSharpTypeSyntax}");
    }

    /// <summary>
    /// Resolves the exported named type (class or enum) for the given type. Callers should assert the type
    /// is TSExport (or its innermost type is) before calling; throws if the type is not a registered named type.
    /// </summary>
    internal NamedTypeInfo GetNamedTypeInfo(InteropTypeInfo type)
    {
        _typeToNamedTypeDict.TryGetValue(type, out NamedTypeInfo? info);
        return info ?? throw new NotFoundClassInfoException($"Could not find NamedTypeInfo for type: {type.CSharpTypeSyntax}");
    }
}
