using Microsoft.CodeAnalysis;

namespace TypeShim.Shared;

internal static class AttributeFacts
{
    private const string JSExportFqn = "global::System.Runtime.InteropServices.JavaScript.JSExportAttribute";
    private const string TSExportFqn = "global::TypeShim.TSExportAttribute";

    /// <summary>
    /// Returns true when <paramref name="attr"/> is <c>[JSExport]</c> from
    /// <c>System.Runtime.InteropServices.JavaScript</c>.  Uses the fully-qualified
    /// name because that assembly is always present in the partial compilation.
    /// </summary>
    internal static bool IsJSExportAttribute(AttributeData attr)
        => attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == JSExportFqn;

    /// <summary>
    /// Returns true when <paramref name="attr"/> is <c>[TSExport]</c> from TypeShim.
    /// When TypeShim.dll is not in the compilation (partial compilation), the attribute
    /// is an unresolved error type; in that case name-only matching is used as a fallback.
    /// </summary>
    internal static bool IsTSExportAttribute(AttributeData attr)
    {
        if (attr.AttributeClass is null) return false;
        if (attr.AttributeClass.Kind == SymbolKind.ErrorType)
            return attr.AttributeClass.Name is "TSExportAttribute" or "TSExport";
        return attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == TSExportFqn;
    }
}
