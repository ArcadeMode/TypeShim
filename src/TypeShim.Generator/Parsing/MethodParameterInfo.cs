using TypeShim.Shared;

internal class MethodParameterInfo
{
    internal required string Name { get; init; }
    internal required bool IsInjectedInstanceParameter { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    internal ParameterDefaultInfo? Default { get; init; }
}

internal sealed record ParameterDefaultInfo(object? Value, bool IsDefaultLiteral);
