using System;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class ModuleHierarchyInfo
{
    internal ClassInfo? ExportedClass { get; private set; }
    internal IReadOnlyDictionary<string, ModuleHierarchyInfo> Children => _children;

    private readonly Dictionary<string, ModuleHierarchyInfo> _children = [];

    internal static ModuleHierarchyInfo FromClasses(IEnumerable<ClassInfo> classInfos)
    {
        ModuleHierarchyInfo moduleInfo = new();
        foreach (ClassInfo classInfo in classInfos)
        {
            string[] namespaceParts = classInfo.Namespace.Split('.', StringSplitOptions.RemoveEmptyEntries);
            ModuleHierarchyInfo namespaceNode = namespaceParts.Aggregate(moduleInfo, static (node, part) => node.GetOrAddChild(part));
            namespaceNode.AddClass(classInfo);
        }
        return moduleInfo;
    }

    // Adds this class as a child interop node, then recurses its nested classes so they nest underneath.
    private void AddClass(ClassInfo classInfo)
    {
        ModuleHierarchyInfo classNode = GetOrAddChild(RenderConstants.InteropClassName(classInfo));
        classNode.ExportedClass = classInfo;
        // Nested enums cross the boundary as numbers and expose no interop methods, so only classes are nested here.
        foreach (ClassInfo nestedClass in classInfo.NestedTypes.OfType<ClassInfo>())
        {
            classNode.AddClass(nestedClass);
        }
    }

    private ModuleHierarchyInfo GetOrAddChild(string key)
    {
        if (!_children.TryGetValue(key, out ModuleHierarchyInfo? child))
        {
            child = new ModuleHierarchyInfo();
            _children[key] = child;
        }
        return child;
    }
}
