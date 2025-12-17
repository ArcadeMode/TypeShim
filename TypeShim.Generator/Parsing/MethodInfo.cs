using Microsoft.CodeAnalysis;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> MethodParameters { get; init; }
    internal required InteropTypeInfo ReturnType { get; init; }

    /// <summary>
    /// A collection of method overloads that share the same public name but differ in their parameter types.
    /// </summary>
    internal required IEnumerable<MethodOverloadInfo> Overloads { get; init; }

    internal bool IsSnapshotOverloaded() => MethodParameters.Any(p => !p.IsInjectedInstanceParameter && p.Type.RequiresCLRTypeConversion) && Overloads.Any();

    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Where(p => !p.IsInjectedInstanceParameter)],
            ReturnType = this.ReturnType,
            Overloads = this.Overloads,
        };
    }

    public MethodInfo WithInteropTypeInfo()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Select(p => p.WithInteropTypeInfo())],
            ReturnType = this.ReturnType.AsInteropTypeInfo(),
            Overloads = this.Overloads,
        };
    }
}

internal sealed class MethodOverloadInfo
{
    internal required string Name { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> MethodParameters { get; init; }
}
