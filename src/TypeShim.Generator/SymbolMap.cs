using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class SymbolMap(IEnumerable<ClassInfo> allClasses)
{
    private readonly Dictionary<InteropTypeInfo, ClassInfo> _typeToClassDict = allClasses.ToDictionary(c => c.Type);

    internal ClassInfo GetClassInfo(InteropTypeInfo type)
    {
        _typeToClassDict.TryGetValue(type, out ClassInfo? info);
        return info ?? throw new NotFoundClassInfoException($"Could not find ClassInfo for type: {type.CSharpTypeSyntax}");
    }
}
