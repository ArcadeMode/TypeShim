using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class SymbolMap(IEnumerable<NamedTypeInfo> allNamedTypes)
{
    private readonly Dictionary<InteropTypeInfo, NamedTypeInfo> _typeToNamedTypeDict = allNamedTypes.ToDictionary(n => n.Type);

    /// <summary>
    /// Resolves the exported named type (class or enum) for the given type. Callers should assert the type
    /// is TSExport (or its innermost type is) before calling; throws if the type is not a registered named type.
    /// </summary>
    internal NamedTypeInfo GetNamedTypeInfo(InteropTypeInfo type)
    {
        _typeToNamedTypeDict.TryGetValue(type, out NamedTypeInfo? info);
        return info ?? throw new NotFoundNamedTypeInfoException($"Could not find NamedTypeInfo for type: {type.CSharpTypeSyntax}");
    }

    /// <summary>
    /// True when the type requires marshalling conversion and its innermost element type is an exported class or a delegate.
    /// Enums also require conversion but cross as their underlying number, so they are deliberately excluded.
    /// </summary>
    internal bool IsConversionRequiringClassOrDelegate(InteropTypeInfo type)
    {
        if (type is not { RequiresTypeConversion: true, SupportsTypeConversion: true })
        {
            return false;
        }

        InteropTypeInfo innermost = type.GetInnermostType();
        bool isExportedClass = _typeToNamedTypeDict.TryGetValue(innermost, out NamedTypeInfo? info) && info is ClassInfo;
        return isExportedClass || innermost.IsDelegateType();
    }
}
