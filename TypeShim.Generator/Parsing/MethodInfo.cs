using Microsoft.CodeAnalysis;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> Parameters { get; init; }
    internal required InteropTypeInfo ReturnType { get; init; }

    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            Parameters = [.. this.Parameters.Where(p => !p.IsInjectedInstanceParameter)],
            ReturnType = this.ReturnType,
        };
    }

    public MethodInfo WithInteropTypeInfo()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            Parameters = [.. this.Parameters.Select(p => p.WithInteropTypeInfo())],
            ReturnType = this.ReturnType.AsInteropTypeInfo(),
        };
    }
}