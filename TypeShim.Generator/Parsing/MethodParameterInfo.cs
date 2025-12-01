using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Generator.Parsing;

internal class MethodParameterInfo
{ 
    internal required string ParameterName { get; init; }
    internal required bool IsInjectedInstanceParameter { get; init; }
    internal required InteropTypeInfo Type { get; init; }

    internal MethodParameterInfo WithoutTypeInfo()
    {
        return new MethodParameterInfo
        {
            ParameterName = this.ParameterName,
            IsInjectedInstanceParameter = this.IsInjectedInstanceParameter,
            Type = InteropTypeInfo.CLRObjectTypeInfo,
        };
    }
}
