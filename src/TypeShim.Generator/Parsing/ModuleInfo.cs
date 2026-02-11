using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class ModuleInfo
{
    internal required IEnumerable<ClassInfo> ExportedClasses { get; init; }

    internal required ModuleHierarchyInfo HierarchyInfo { get; init; }
}
