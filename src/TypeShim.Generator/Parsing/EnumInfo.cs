namespace TypeShim.Generator.Parsing;

internal sealed class EnumInfo : NamedTypeInfo
{
    internal required string UnderlyingType { get; init; }
    internal required IReadOnlyList<EnumMemberInfo> Members { get; init; }

    internal string? GetMemberByValue(long value) =>
        Members.FirstOrDefault(m => m.Value == value)?.Name;
}

internal sealed class EnumMemberInfo
{
    internal required string Name { get; init; }
    internal required long Value { get; init; }
    internal required CommentInfo? Comment { get; init; }
}
