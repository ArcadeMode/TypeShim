namespace TypeShim.Generator.Parsing;

internal sealed class EnumMemberInfo
{
    internal required string Name { get; init; }
    internal required long Value { get; init; }
    internal required CommentInfo? Comment { get; init; }
}

internal sealed class EnumInfo : NamedTypeInfo
{
    /// <summary>
    /// The C# keyword for the enum's underlying integral type (e.g. "int", "uint").
    /// </summary>
    internal required string UnderlyingType { get; init; }

    /// <summary>
    /// Enum members in declaration order, with their exact constant values.
    /// </summary>
    internal required IReadOnlyList<EnumMemberInfo> Members { get; init; }
}
