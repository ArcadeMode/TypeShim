using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class SymbolMap
{
    private readonly Dictionary<InteropTypeInfo, NamedTypeInfo> _typeToNamedTypeDict = [];
    // Maps each named type to its immediate enclosing named type. Absence of a key means the type is top-level.
    private readonly Dictionary<NamedTypeInfo, NamedTypeInfo> _parent = [];

    internal SymbolMap(IEnumerable<NamedTypeInfo> allNamedTypes)
    {
        foreach (NamedTypeInfo namedType in allNamedTypes)
        {
            Index(namedType, parent: null);
        }
    }

    // Recursively indexes a named type and all of its nested types so that references to nested types
    // (including from other classes, e.g. A referencing B.C) resolve, and the nesting hierarchy is preserved.
    private void Index(NamedTypeInfo node, NamedTypeInfo? parent)
    {
        _typeToNamedTypeDict[node.Type] = node;
        if (parent is not null)
        {
            _parent[node] = parent;
        }
        foreach (NamedTypeInfo nested in node.NestedTypes)
        {
            Index(nested, node);
        }
    }

    /// <summary>
    /// Resolves the exported named type (class or enum) for the given type. Callers should assert the type
    /// is TSExport (or its innermost type is) before calling; throws if the type is not a registered named type.
    /// </summary>
    internal NamedTypeInfo GetNamedTypeInfo(InteropTypeInfo type)
    {
        _typeToNamedTypeDict.TryGetValue(type, out NamedTypeInfo? info);
        return info ?? throw new NotFoundNamedTypeInfoException($"Could not find NamedTypeInfo for type: {type.CSharpFullyQualifiedTypeSyntax}");
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

    /// <summary>
    /// The user-facing TypeScript reference for a named type, dotted through the nesting hierarchy
    /// (e.g. <c>Shipment.Manifest</c> for a nested type, or <c>Shipment</c> for a top-level type).
    /// </summary>
    internal string GetTypeScriptReferenceName(InteropTypeInfo type)
        => string.Join(".", AncestorChainAndSelf(GetNamedTypeInfo(type)).Select(n => n.Name));

    /// <summary>
    /// The TypeScript accessor for a class's interop members on the <c>AssemblyExports</c> object,
    /// e.g. <c>N1.ShipmentInterop.ManifestInterop</c>.
    /// </summary>
    internal string GetTypeScriptInteropAccessor(ClassInfo classInfo)
    {
        string dottedInterop = string.Join(".", AncestorChainAndSelf(classInfo).Select(InteropSegmentName));
        return string.IsNullOrEmpty(classInfo.Namespace) ? dottedInterop : $"{classInfo.Namespace}.{dottedInterop}";
    }

    /// <summary>
    /// The fully-qualified (<c>global::</c>-prefixed) C# interop class name for a possibly-nested type,
    /// suffixing every enclosing exported segment with <c>Interop</c> (e.g. <c>global::N1.ShipmentInterop.ManifestInterop</c>).
    /// </summary>
    internal string GetCSharpInteropTypeName(InteropTypeInfo type)
    {
        NamedTypeInfo node = GetNamedTypeInfo(type);
        string dottedInterop = string.Join(".", AncestorChainAndSelf(node).Select(InteropSegmentName));
        return string.IsNullOrEmpty(node.Namespace) ? $"global::{dottedInterop}" : $"global::{node.Namespace}.{dottedInterop}";
    }

    // Returns the enclosing named types from outermost to the type itself.
    private IReadOnlyList<NamedTypeInfo> AncestorChainAndSelf(NamedTypeInfo node)
    {
        List<NamedTypeInfo> chain = [];
        for (NamedTypeInfo? current = node; current is not null; current = _parent.GetValueOrDefault(current))
        {
            chain.Add(current);
        }
        chain.Reverse();
        return chain;
    }

    // Classes carry an Interop suffix; enums do not (they never enclose types and cross as their underlying number).
    private static string InteropSegmentName(NamedTypeInfo node)
        => node is ClassInfo classInfo ? RenderConstants.InteropClassName(classInfo) : node.Name;
}
