using Microsoft.CodeAnalysis;

namespace TypeShim.Analyzers;

internal static class TypeShimDiagnostics
{
    internal static readonly DiagnosticDescriptor AttributeMutexRule = new(
        id: "TSHIM001",
        title: "TSModule and TSExport cannot be applied together",
        messageFormat: "Class '{0}' has both [TSModule] and [TSExport]; these attributes are mutually exclusive",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TSModule classes are entry points; TSExport marks user classes. They cannot be combined.");

    internal static readonly DiagnosticDescriptor TSModuleMustBeStaticRule = new(
        id: "TSHIM002",
        title: "TSModule can only be applied to static classes",
        messageFormat: "Classes with [TSModule] must be static",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[TSModule] must be applied to static classes.");

    internal static readonly DiagnosticDescriptor TSExportMustBeNonStaticRule = new(
        id: "TSHIM003",
        title: "TSExport can only be applied to non-static classes",
        messageFormat: "Classes with [TSExport] must be non-static",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[TSExport] must be applied to non-static classes.");

    internal static readonly DiagnosticDescriptor AttributeOnPublicClassOnlyRule = new(
        id: "TSHIM008",
        title: "TSModule and TSExport can only be applied to classes with public accessibility",
        messageFormat: "'{0}' has either [TSModule] or [TSExport] and is not a class or has non-public accessibility",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TSModule and TSExport are only supported on classes with public accessibility.");

    internal static readonly DiagnosticDescriptor TSExportStaticMembersRule = new(
        id: "TSHIM004",
        title: "Static members on TSExport classes are not exported",
        messageFormat: "Public static member '{0}' will not be TSExported, consider making it non-static, decreasing its accessibility or moving it to a [TSModule]",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "TSExport classes expose instance members in TypeScript; static members are not exported.");

    internal static readonly DiagnosticDescriptor UnsupportedTypeRule = new(
        id: "TSHIM005",
        title: "Unsupported type pattern",
        messageFormat: "Type '{0}' is not supported by TypeShim nor .NET-JS type marshalling",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The provided type is currently unsupported by .NET Type Marshalling and/or TypeShim interop.");

    internal static readonly DiagnosticDescriptor NonExportedTypeInMethodRule = new(
        id: "TSHIM006",
        title: "Non-TSExport type on a method in the interop API",
        messageFormat: "Type '{0}' is not annotated with [TSExport], it will be exported to TypeScript as 'object'",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Interop APIs should use TSExport-annotated classes or supported primitives as return- and parameter types.");

    internal static readonly DiagnosticDescriptor NonExportedTypeInPropertyRule = new(
        id: "TSHIM009",
        title: "Non-TSExport type on a property in the interop API",
        messageFormat: "Type '{0}' is not annotated with [TSExport], properties cannot contain unmarshallable types",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Interop APIs should use TSExport-annotated classes or supported primitives as property types.");

    internal static readonly DiagnosticDescriptor UnderDevelopmentTypeRule = new(
        id: "TSHIM007",
        title: "Type under development",
        messageFormat: "Type '{0}' is not yet implemented by TypeShim",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "This type is under development and not yet supported by TypeShim interop.");

    internal static readonly DiagnosticDescriptor NonPublicSetterRule = new(
        id: "TSHIM010",
        title: "Non-public set/init will be skipped during JSObject mapping",
        messageFormat: "Property '{0}' has a non-public setter/init accessor and will be skipped in JSObject to '{1}' mapping",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "TypeShim generates a JSObject mapper for each TSExport class. Properties with non-public set/init accessors are excluded from the generated JSObject mapper.");

}
