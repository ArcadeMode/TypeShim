using Microsoft.CodeAnalysis;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required bool IsConstructor { get; init; }
    internal required string Name { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> MethodParameters { get; init; }
    internal required InteropTypeInfo ReturnType { get; init; }

    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            IsConstructor = this.IsConstructor,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Where(p => !p.IsInjectedInstanceParameter)],
            ReturnType = this.ReturnType,
        };
    }

    public MethodInfo WithInteropTypeInfo()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            IsConstructor = this.IsConstructor,
            Name = this.Name,
            MethodParameters = [.. this.MethodParameters.Select(p => p.WithInteropTypeInfo())],
            ReturnType = this.ReturnType.AsInteropTypeInfo(),
        };
    }
}