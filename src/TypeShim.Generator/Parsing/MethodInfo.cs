using Microsoft.CodeAnalysis;
using System.Reflection;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> Parameters { get; init; }
    internal required InteropTypeInfo ReturnType { get; init; }
    internal required CommentInfo? Comment { get; init; }

    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            Parameters = [.. this.Parameters.Where(p => !p.IsInjectedInstanceParameter)],
            ReturnType = this.ReturnType,
            Comment = this.Comment,
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
            Comment = this.Comment,
        };
    }

    internal bool MatchesDisposeSignature()
    {
        return Name == "Dispose" && !Parameters.Any(p => !p.IsInjectedInstanceParameter) && ReturnType.ManagedType == KnownManagedType.Void;
    }
}