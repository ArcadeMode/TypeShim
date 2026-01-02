using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace TypeShim.Analyzers;

internal static class TypeShimDiagnostics
{
    internal static readonly DiagnosticDescriptor UnsupportedTypeRule = new(
        id: "TSHIM005",
        title: "Unsupported type pattern",
        messageFormat: "Type '{0}' is not supported by TypeShim nor .NET-JS type marshalling",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The provided type is currently unsupported by .NET Type Marshalling and/or TypeShim interop.");

    internal static readonly DiagnosticDescriptor NonExportedTypeInInteropApiRule = new(
        id: "TSHIM006",
        title: "Non-TSExport type on a method in the interop API",
        messageFormat: "Type '{0}' has no [TSExport] annotation, it will present in TypeScript as 'ManagedObject'",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "TSExport public API should use supported .NET-JS interop types or (other) TSExport classes.");

    internal static readonly DiagnosticDescriptor UnderDevelopmentTypeRule = new(
        id: "TSHIM007",
        title: "Type under development",
        messageFormat: "Type '{0}' is not yet implemented in TypeShim",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "This type is under development and not yet implemented in TypeShim.");

    internal static readonly DiagnosticDescriptor AttributeOnPublicClassOnlyRule = new(
        id: "TSHIM008",
        title: "TSExport can only be used on public classes",
        messageFormat: "TSExport '{0}' must have public accessibility",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TSExport is only supported on classes with public accessibility.");

    internal static readonly DiagnosticDescriptor NoOverloadsRule = new(
        id: "TSHIM009",
        title: "Overloading is not supported",
        messageFormat: "Public member overloading is not supported, consider decreasing the number of public overloads of '{0}' by lowering accessibility or changing names",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TypeShim does not support fields (yet), they're mostly ignored but required fields are banned to prevent invalid constructor initializers from being generated.");

    internal static readonly DiagnosticDescriptor NonPublicSetterRule = new(
        id: "TSHIM010",
        title: "Non-public set/init will be skipped during JSObject mapping",
        messageFormat: "Property '{0}' has a non-public set/init, it will not be available in the TypeScript constructor of class '{1}'",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "TypeShim generates initializer objects to use in conjuction with constructors and for automatic JSObject mapping. Properties with non-public set/init's are excluded from this feature.");

    internal static readonly DiagnosticDescriptor NoRequiredFieldsRule = new(
        id: "TSHIM011",
        title: "Required fields are not supported",
        messageFormat: "Required fields are not supported, consider making '{0}' a property or an optional field",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TypeShim does not support fields (yet), they're mostly ignored but required fields are banned to prevent invalid constructor initializers from being generated.");

}
