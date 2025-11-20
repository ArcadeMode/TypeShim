namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class WasmModuleInfo
{
    internal required ClassInfo? ExportedClass { get; init; }
    internal IReadOnlyDictionary<string, WasmModuleInfo> Children => _children;

    private readonly Dictionary<string, WasmModuleInfo> _children = [];

    internal static WasmModuleInfo FromClasses(IEnumerable<ClassInfo> classInfos)
    {
        WasmModuleInfo moduleInfo = new() { ExportedClass = null };
        foreach (ClassInfo classInfo in classInfos)
        {
            string[] propertyAccessorParts = [.. classInfo.Namespace.Split('.'), classInfo.Name];
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
            _children[localExport] = new WasmModuleInfo
            {
                ExportedClass = classInfo
            };
            return;
        }
        else
        {
            string localExport = accessorParts[0];
            string[] remainingParts = accessorParts[1..];
            if (!_children.TryGetValue(localExport, out WasmModuleInfo? value))
            {
                value = new WasmModuleInfo
                {
                    ExportedClass = null
                };
                _children[localExport] = value;
            }

            value.Add(remainingParts, classInfo);
        }
    }
}
