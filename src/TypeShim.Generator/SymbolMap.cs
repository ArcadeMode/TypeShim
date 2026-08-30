using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class SymbolMap(IEnumerable<ClassInfo> allClasses)
{
    private readonly Dictionary<InteropTypeInfo, NamedTypeInfo> _typeToNamedTypeDict = allClasses.ToDictionary(c => c.Type, c => (NamedTypeInfo)c);

    internal ClassInfo GetClassInfo(InteropTypeInfo type)
    {
        _typeToNamedTypeDict.TryGetValue(type, out NamedTypeInfo? info);
        return info as ClassInfo ?? throw new NotFoundClassInfoException($"Could not find ClassInfo for type: {type.CSharpTypeSyntax}");
    }
}
