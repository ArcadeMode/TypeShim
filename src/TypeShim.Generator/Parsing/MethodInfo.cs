using Microsoft.CodeAnalysis;
using System.Reflection;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required MethodParameterInfo? InstanceParameter { get; init; }
    internal required IReadOnlyCollection<MethodParameterInfo> Parameters { get; init; }
    internal required InteropTypeInfo ReturnType { get; init; }
    internal required CommentInfo? Comment { get; init; }

    internal IReadOnlyCollection<MethodParameterInfo> GetParametersIncludingInstanceParameter()
    {
        if (InstanceParameter is MethodParameterInfo instanceParameter)
        {
            return [instanceParameter, .. Parameters];
        }
        return Parameters;
    }

    internal bool MatchesDisposeSignature()
    {
        return Name == "Dispose" && Parameters.Count == 0 && ReturnType.ManagedType == KnownManagedType.Void;
    }
}