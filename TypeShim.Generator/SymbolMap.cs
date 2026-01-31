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

    //internal string GetUserClassSymbolName(InteropTypeInfo type, TypeShimSymbolType flags)
    //{
    //    ClassInfo classInfo = GetClassInfo(type.GetInnermostType());
    //    return GetUserClassSymbolNameCore(type, classInfo.Type, flags);
    //}

    //internal string GetUserClassSymbolName(ClassInfo classInfo, TypeShimSymbolType flags)
    //{
    //    return GetUserClassSymbolNameCore(classInfo.Type, classInfo.Type, flags);
    //}

    //internal string GetUserClassSymbolName(ClassInfo classInfo, InteropTypeInfo useSiteTypeInfo, TypeShimSymbolType flags)
    //{
    //    return TypeScriptSymbolNameRenderer.Render(useSiteTypeInfo, classInfo.Type, flags);
    //}

    //private static string GetUserClassSymbolNameCore(InteropTypeInfo useSiteTypeInfo, InteropTypeInfo userTypeInfo, TypeShimSymbolType flags)
    //{
    //    return (flags) switch
    //    {
    //        TypeShimSymbolType.Proxy => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
    //        TypeShimSymbolType.Namespace => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
    //        TypeShimSymbolType.Snapshot => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Properties}"),
    //        TypeShimSymbolType.Initializer => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}"),
    //        TypeShimSymbolType.ProxyInitializerUnion => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $" | {userTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}")}"),
    //        _ => throw new NotImplementedException(),
    //    };
    //}
}
