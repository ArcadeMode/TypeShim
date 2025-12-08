using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Generator.Parsing;

internal class MethodParameterInfo
{ 
    internal required string Name { get; init; }
    internal required bool IsInjectedInstanceParameter { get; init; }
    internal required InteropTypeInfo Type { get; init; }

    internal MethodParameterInfo WithInteropTypeInfo()
    {
        return new MethodParameterInfo
        {
            Name = this.Name,
            IsInjectedInstanceParameter = this.IsInjectedInstanceParameter,
            Type = Type.AsInteropTypeInfo(),
        };
    }
}
