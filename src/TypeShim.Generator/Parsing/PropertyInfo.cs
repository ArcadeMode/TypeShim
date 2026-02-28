using TypeShim.Shared;
using TypeShim.Generator.Parsing;

internal sealed class PropertyInfo
{
    internal required string Name { get; init; }
    internal required bool IsStatic { get; init; }
    internal required bool IsRequired { get; init; }
    internal required InteropTypeInfo Type { get; init; }

    internal required MethodInfo GetMethod { get; init; }
    internal required MethodInfo? SetMethod { get; init; }
    internal required MethodInfo? InitMethod { get; init; }
    
    internal required CommentInfo? Comment { get; init; }
}