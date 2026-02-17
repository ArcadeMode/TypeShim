namespace TypeShim.Generator.Parsing;

internal sealed class CommentInfo
{
    internal required string Description { get; init; }
    internal required IReadOnlyCollection<ParameterCommentInfo> Parameters { get; init; }
    internal required string? Returns { get; init; }
    internal required IReadOnlyCollection<ThrowsCommentInfo> Throws { get; init; }

    internal bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Description) && 
               Parameters.Count == 0 && 
               string.IsNullOrWhiteSpace(Returns) && 
               Throws.Count == 0;
    }
}
