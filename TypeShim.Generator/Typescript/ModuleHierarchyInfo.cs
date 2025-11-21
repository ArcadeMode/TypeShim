using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class ModuleInfo
{
    internal required IEnumerable<ClassInfo> ExportedClasses { get; init; }

    internal required ModuleHierarchyInfo HierarchyInfo { get; init; }
}

internal class ModuleHierarchyInfo
{
    internal required ClassInfo? ExportedClass { get; init; }
    internal IReadOnlyDictionary<string, ModuleHierarchyInfo> Children => _children;

    private readonly Dictionary<string, ModuleHierarchyInfo> _children = [];

    internal static ModuleHierarchyInfo FromClasses(IEnumerable<ClassInfo> classInfos, TypescriptClassNameBuilder classNameBuilder)
    {
        ModuleHierarchyInfo moduleInfo = new() { ExportedClass = null };
        foreach (ClassInfo classInfo in classInfos)
        {
            string[] propertyAccessorParts = [.. classInfo.Namespace.Split('.'), classNameBuilder.GetInteropInterfaceName(classInfo)];
            moduleInfo.Add(propertyAccessorParts, classInfo);
        }
        return moduleInfo;
    }

    private void Add(string[] accessorParts, ClassInfo classInfo)
    {
        if (accessorParts.Length == 0)
        {
            throw new InvalidOperationException("Cannot add class with no namespace parts");
        }

        if (accessorParts.Length == 1)
        {
            string localExport = accessorParts[0];
            _children[localExport] = new ModuleHierarchyInfo
            {
                ExportedClass = classInfo
            };
            return;
        }
        else
        {
            string localExport = accessorParts[0];
            string[] remainingParts = accessorParts[1..];
            if (!_children.TryGetValue(localExport, out ModuleHierarchyInfo? value))
            {
                value = new ModuleHierarchyInfo
                {
                    ExportedClass = null
                };
                _children[localExport] = value;
            }

            value.Add(remainingParts, classInfo);
        }
    }
}
