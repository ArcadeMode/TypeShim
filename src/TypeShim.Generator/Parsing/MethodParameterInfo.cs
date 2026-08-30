using TypeShim.Shared;

internal class MethodParameterInfo
{
    internal required string Name { get; init; }
    internal required bool IsInjectedInstanceParameter { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    internal ParameterDefaultInfo? Default { get; init; }
}

/// <summary>
/// Describes the default value of an optional parameter.
/// </summary>
/// <param name="Value">
/// The resolved compile-time constant value (may be <c>null</c> for reference/nullable defaults or the <c>default</c> literal).
/// </param>
/// <param name="IsDefaultLiteral">
/// <c>true</c> when the default was written as <c>default</c>/<c>default(T)</c>, which is distinct from an explicit <c>= null</c>.
/// </param>
internal sealed record ParameterDefaultInfo(object? Value, bool IsDefaultLiteral);
