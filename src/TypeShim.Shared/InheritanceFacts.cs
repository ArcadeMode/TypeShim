using Microsoft.CodeAnalysis;

namespace TypeShim.Shared;

/// <summary>
/// Shared reasoning about the inheritance relationships of a [TSExport] class. TypeShim does not
/// (yet) support inheritance: base-class members are not projected into the generated interop, which
/// can produce uncompilable code (e.g. a required base property never set in the generated
/// initializer). Only <see cref="SpecialType.System_IDisposable"/> is allowed, because a class's
/// <c>Dispose()</c> is picked up as an ordinary declared method and the interface itself contributes
/// no additional projected surface.
/// </summary>
internal static class InheritanceFacts
{
    /// <summary>
    /// Returns the base class or interface that makes <paramref name="type"/> an unsupported
    /// inheritor, or <c>null</c> when the type only inherits from <see cref="object"/> and/or
    /// <see cref="System.IDisposable"/>.
    /// </summary>
    /// <remarks>
    /// Only the presence of an inheritance relationship is inspected, never inherited members. This
    /// keeps the check reliable under the generator's restricted partial compilation, where a
    /// non-exported base degrades to an <see cref="TypeKind.Error"/> type but still exposes its name
    /// and special type. Directly declared interfaces are considered (not the transitive closure) so
    /// that an interface which merely extends <see cref="System.IDisposable"/> is still rejected.
    /// </remarks>
    internal static ISymbol? GetUnsupportedBaseOrInterface(INamedTypeSymbol type)
    {
        if (type.BaseType is INamedTypeSymbol baseType && baseType.SpecialType != SpecialType.System_Object)
        {
            return baseType;
        }

        foreach (INamedTypeSymbol interfaceType in type.Interfaces)
        {
            if (interfaceType.SpecialType != SpecialType.System_IDisposable)
            {
                return interfaceType;
            }
        }

        return null;
    }
}
