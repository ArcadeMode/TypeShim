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
    internal required IEnumerable<MethodInfo> SnapshotOverloads { get; init; } 
    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Where(p => !p.IsInjectedInstanceParameter)],
            ReturnType = this.ReturnType,
            SnapshotOverloads = this.SnapshotOverloads,
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
            SnapshotOverloads = this.SnapshotOverloads,
        };
    }
}
